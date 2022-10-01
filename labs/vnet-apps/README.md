

## Create RG, VNet and Subnet

```
az group create -n labs-vnet-apps2 --tags courselabs=azure 

az network vnet create -g labs-vnet-apps2 -n vnet1 --address-prefix "10.30.0.0/16"

az network vnet subnet create -g labs-vnet-apps2 --vnet-name vnet1 -n subnet1 --address-prefix "10.30.1.0/24"
```

## Create Storage Account and KeyVault 

```
az storage account create -g labs-vnet-apps2 --sku Standard_ZRS  -n labsvnetappses #<sa-name>

az keyvault create -g labs-vnet-apps2 -n labsvnetappses #<kv-name>
```

Store the connection string for blob storage in the KeyVault:

```
# check the key:
az storage account show-connection-string -o tsv --name labsvnetappses -g labs-vnet-apps2

# in PowerShell:
$connectionString=$(az storage account show-connection-string -o tsv --name labsvnetappses -g labs-vnet-apps2)

# or in *sh:
connectionString=$(az storage account show-connection-string -o tsv --name labsvnetappses -g labs-vnet-apps2)

# store it as a keyvault secret:
az keyvault secret set --name 'ConnectionStrings--AssetsDb' --value $connectionString --vault-name <kv-name>
```

Check in Portal that you can read the connection string in the secret.

## Restrict Access

Set the subnet so it can connect to storage and keyvault:

```
az network vnet subnet update -g labs-vnet-apps2 --vnet-name vnet1 --name subnet1 --service-endpoints Microsoft.KeyVault Microsoft.Storage
```

Now restrict the KeyVault so it's only accessible from the vnet:

```
az keyvault network-rule add --vnet-name vnet1 --subnet subnet1 -g labs-vnet-apps2 --name vnetappses #<kv-name>

az keyvault update --default-action 'Deny' -g labs-vnet-apps2 -n vnetappses #<kv-name>

az keyvault network-rule list -g labs-vnet-apps2 --name vnetappses #<kv-name>
```

Check the portal again and see if you can read the secret value; verify the firewall settings in the _Networking_ tab


## Create Web App using VNet, KeyVault and Blob Storage

```
# need standard SKU for vnet integration
az appservice plan create -g labs-vnet-apps2 -n app-plan-01 --is-linux --sku S1 --number-of-workers 1

cd src/asset-manager

az webapp up -g labs-vnet-apps2 --plan app-plan-01 --os-type Linux --runtime dotnetcore:6.0 -n assetmanageres # dns name 


az webapp config appsettings set -g labs-vnet-apps2 --settings Database__Api=BlobStorage KeyVault__Enabled=true KeyVault__Name=<keyvault-name> -n  assetmanageres #<dns-unique-app-name>
```

Browse to the app - it will show an error page, find your way to the logs and you'll see an error connecting to KeyVault - the app isn't using an identity which KeyVault truests


Use a managed identity and grant it permission to the KeyVault:

```
# output contains the id of the identity:
az webapp identity assign -g labs-vnet-apps2  -n assetmanageres #<dns-unique-app-name>

# give the identity access to read secrets:
az keyvault set-policy  --object-id "<principalId>" --secret-permissions get -n <kv-name>

az keyvault set-policy --object-id e6ff5d2c-f1f1-4ce9-93cf-73c2d265091d --secret-permissions get -g labs-vnet-apps2  -n vnetappses
```

Try again...

Logs show

(Inner Exception #1) Azure.Identity.CredentialUnavailableException: ManagedIdentityCredential authentication unavailable. Multiple attempts failed to obtain a token from the managed identity endpoint.
2022-10-01T11:23:03.896054325Z  ---> System.AggregateException: Retry failed after 4 tries. Retry settings can be adjusted in ClientOptions.Retry. (Connection refused (169.254.169.254:80)) (Connection refused (169.254.169.254:80)) (Connection refused (169.254.169.254:80)) (Connection refused (169.254.169.254:80))

## Lab
TODO - can't use vnet integration in case of free accounts; stick with Managed Identity
