# Securing Apps with Key Vault and Virtual Networks

The ideal application in Azure uses managed identities for all authentication and restricted virtual networks for all communication. That way there are no credentials to manage and store, and no services exposed beyond where they should be accessible. But... not all services in Azure support VNet connections, and not all components in your app will have integrated authentication.

In this lab we'll deploy an app which uses Blob Storage, storing the connection details in a KeyVault which is restricted to a VNet.

## Reference

- [Storage Account firewall and Virtual Networks](https://learn.microsoft.com/en-us/azure/storage/common/storage-network-security?tabs=azure-portal)

- [VNet integration for App Service apps](https://learn.microsoft.com/en-us/azure/app-service/overview-vnet-integration)


## Create RG, VNet and Subnet

Let's start with the core resources - the RG and VNet:

```
az group create -n labs-vnet-apps --tags courselabs=azure 

az network vnet create -g labs-vnet-apps -n vnet1 --address-prefix "10.30.0.0/16"

az network vnet subnet create -g labs-vnet-apps --vnet-name vnet1 -n subnet1 --address-prefix "10.30.1.0/24"
```

Nothing new here. We're not actually going to deploy anything into the VNet - we'll use it as a bridge to secure communication between services.

## Create Storage Account and KeyVault 

The app uses Blob Storage, so we'll need to create an account and grab the connection string. This application has code to create the blob container, so we don't need to do that in advance.

```
# create the account:
az storage account create -g labs-vnet-apps --sku Standard_ZRS -n <sa-name>

# print the connection string:
az storage account show-connection-string -o tsv -g labs-vnet-apps --name <sa-name> 
```

That key gives complete access to all everything in the Storage Account, so we need to keep it safe. We'll create a KeyVault and store the connection string in a secret:

```
# create the vault:
az keyvault create -g labs-vnet-apps -n <kv-name> 

# store the secret:
az keyvault secret set --name 'ConnectionStrings--AssetsDb'  --vault-name <kv-name> --value "<connection-string>"
```

📋 Check you can read the secret from your machine.

<details>
  <summary>Not sure how?</summary>

```
az keyvault secret show --name 'ConnectionStrings--AssetsDb'  --vault-name <kv-name>
```

</details>

There's no need for this secret to be accessible outside of Azure, so we should lock it down.

## Restrict Access

We'll use the subnet for communication to KeyVault and Storage, so we need to set service endpoints to allow that:

```
az network vnet subnet update -g labs-vnet-apps --vnet-name vnet1 --name subnet1 --service-endpoints Microsoft.KeyVault Microsoft.Storage
```

Now restrict the KeyVault so it's only accessible from the vnet:

```
az keyvault network-rule add --vnet-name vnet1 --subnet subnet1 -g labs-vnet-apps --name <kv-name>

az keyvault update --default-action 'Deny' -g labs-vnet-apps -n <kv-name>

az keyvault network-rule list -g labs-vnet-apps --name <kv-name>
```

Check you can read the secret with the CLI or the Portal again. It may take a few minutes for the rules to take effect, but now the secret should be blocked outside of the VNet.

## Create Web App using VNet, KeyVault and Blob Storage

Our app is a .NET 6 web site, so it's a good fit for PaaS. _App Services don't run inside VNets_ - they're intended to be public facing. We can still secure them, but it needs some more configuration.

Start by deploying the app as a Web App:

```
cd src/asset-manager

az webapp up -g labs-vnet-apps --plan app-plan-02 --os-type Linux --runtime dotnetcore:6.0 --sku B1 -n <app-name>
```

Now set some app configuration settings. These tell the app to use Blob Storage for data, and to fetch the connection string from Key Vault:

```
az webapp config appsettings set -g labs-vnet-apps --settings Database__Api=BlobStorage KeyVault__Enabled=true KeyVault__Name=<kv-name> -n <app-name>
```

Browse to the app - it will show an error page. 

📋 Open the logs for the app and see if you can find the error.

<details>
  <summary>Not sure how?</summary>

Open _Advanced tools_ for the web app in the Portal and launch the Kudu session. Open the _Log stream_ link and be patient...

The app will keep restarting because the failure causes it to exit. You'll eventually see a useful error log like this:

_{"error":{"code":"Forbidden","message":"The user, group or application 'appid=19ee0b80-40d0-4a42-b4ca-b8697c84c6a8;oid=4a09a335-0716-406d-a12f-9cafadae0325;iss=https://sts.windows.net/68c58dc9-c7db-440f-8c32-ac672250d642/' does not have secrets list permission on key vault 'labsvnetappses;location=westeurope'. For help resolving this issue, please see https://go.microsoft.com/fwlink/?linkid=2125287","innererror":{"code":"AccessDenied"}}}_

</details>

The problem is an an error connecting to KeyVault - the app isn't using an identity which KeyVault trusts.

App Service can use managed identity, so it can authenticate with KeyVault without needing any connection strings or other credentials. Set the web app to use a managed identity and grant that identity permission to the KeyVault:

```
# assign the identity - the output contains the id of the identity:
az webapp identity assign -g labs-vnet-apps  -n <app-name>

# give the identity access to read secrets:
az keyvault set-policy --secret-permissions get list --object-id "<principalId>" -n <kv-name>
```

Try the app again... It will fail. 

📋 Open the logs for the app and see if you can find the new error.

<details>
  <summary>Not sure how?</summary>

Same process and same long wait, but you will see a new error: 

_{"error":{"code":"Forbidden","message":"Client address is not authorized and caller is not a trusted service.\r\nClient address: 20.126.176.160\r\nCaller: appid=19ee0b80-40d0-4a42-b4ca-b8697c84c6a8;oid=4a09a335-0716-406d-a12f-9cafadae0325;iss=https://sts.windows.net/68c58dc9-c7db-440f-8c32-ac672250d642/;xmsmirid=/subscriptions/161aa8d6-1b59-4fff-946c-e1172b68d76c/resourcegroups/labs-vnet-apps/providers/Microsoft.Web/sites/app-name;xmsazrid=/subscriptions/161aa8d6-1b59-4fff-946c-e1172b68d76c/resourcegroups/labs-vnet-apps/providers/Microsoft.Web/sites/app-name\r\nVault: labsvnetappses;location=westeurope","innererror":{"code":"ForbiddenByFirewall"}}}_

</details>

Now the App Service is using an authorized identity but the call is not coming from a trusted location, because the KeyVault is restricted to the subnet.

One option we have here is to get the outbound IP addresses of the webapp and add them to the KeyVault firewall. But IP addresses change, so it's better to add VNet integration to the web app. Then when the App Service makes internal Azure calls, it will be via the subnet which has Key Vault access:

```
az webapp vnet-integration add --vnet vnet1 --subnet subnet1 -g labs-vnet-apps  -n <app-name>

# check the app:
az webapp show -g labs-vnet-apps -n <app-name> 
```

Now when the changes filter through, the app can connect to Key Vault where it reads the connection string for the Storage Account and then it downloads the data from the blob container.

## Lab

But the Storage Account is still open to the Internet. Storage Accounts can't be deployed inside a VNet (like Web Apps they're intended to have public connections), but they can be restricted. Fix the Storage Account so only services using the subnet have access.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG with this command to remove all the resources:

```
az group delete -y -n labs-vnet-apps
```