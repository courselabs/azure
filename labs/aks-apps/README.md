# Securing AKS Apps with Key Vault and Virtual Networks

AKS clusters can run inside VNets using the Azure network provider. All the Pods in the cluster will get an IP address from the subnet range. Any Azure resources which support firewall rules can be set to only allow traffic from the subnet, so only AKS Pods can use the service.

In this lab we'll deploy an app to AKS which uses Blob Storage, storing the connection details in a KeyVault, and restricting access to the cluster's subnet.

## Reference

- [Managed Identities in AKS](https://learn.microsoft.com/en-us/azure/aks/use-managed-identity)

- [Storage Account firewall and Virtual Networks](https://learn.microsoft.com/en-us/azure/storage/common/storage-network-security?tabs=azure-portal)

- [KeyVault firewall and Virtual Networks](https://learn.microsoft.com/en-us/azure/key-vault/general/network-security)


## Create RG, VNet and Subnet

Let's start with the core resources - the RG and VNet:

```
az group create -n labs-aks-apps --tags courselabs=azure 

az network vnet create -g labs-aks-apps -n appnet --address-prefix "10.30.0.0/16" -l eastus

az network vnet subnet create -g labs-aks-apps --vnet-name appnet -n aks --address-prefix "10.30.1.0/24"
```

Nothing new here. AKS is a managed service but it is VNet-deployable, so Pods will use an IP address from the subnet. That means we can secure all the other services to only allow access from Pods.

## Create AKS cluster

We need to get the ID of the subnet for AKS:

```
az network vnet subnet show -g labs-aks-apps --vnet-name appnet -n aks --query id -o tsv
```

Now create the cluster using Azure networking and with the KeyVault add-on enabled (we covered this in the [AKS KeyVault lab](/labs/aks-keyvault/README.md)):

```
az aks create -g labs-aks-apps -n aks06 --node-count 2 --enable-addons azure-keyvault-secrets-provider --enable-managed-identity --network-plugin azure --vnet-subnet-id '<subnet-id>' -l eastus
```

> This does the AD role propagation for the VNet which takes a while.

Open a new terminal so you can carry on creating the rest of the infrastructure.

## Create Storage Account and KeyVault 

The app uses Blob Storage, so we'll need to create an account and grab the connection string. This application can create the blob container on startup, but it's good practice to do that in advance.

```
# create the storage account:
az storage account create -g labs-aks-apps --sku Standard_ZRS -l eastus -n <sa-name>

# and container:
az storage container create -n assetsdb -g labs-aks-apps --account-name <sa-name>

# print the connection string:
az storage account show-connection-string -o tsv -g labs-aks-apps --name <sa-name> 
```

**Edit the file [asset-manager-connectionstrings.json](/labs/aks-apps/secrets/asset-manager-connectionstrings.json)** replacing `<sa-connection-string>` with your own connection string.

That key gives complete access to all everything in the Storage Account, so we need to keep it safe. We'll create a KeyVault and upload the connection string file to a secret:

```
# create the vault:
az keyvault create -g labs-aks-apps -l eastus -n <kv-name> 

# store the secret:
az keyvault secret set --name asset-manager-connectionstrings  --file labs/aks-apps/secrets/asset-manager-connectionstrings.json --vault-name <kv-name>
```

📋 Check you can read the secret from your machine.

<details>
  <summary>Not sure how?</summary>

```
az keyvault secret show --name asset-manager-connectionstrings  --vault-name <kv-name>
```

</details>

This secret will be read by the app running in the AKS Pod, but there's no need for it to be accessible outside of Azure, so we should lock it down.

## Restrict KeyVault Access

We'll use the AKS subnet for communication to KeyVault and Storage, so we need to set service endpoints to allow that:

```
az network vnet subnet update -g labs-aks-apps --vnet-name appnet --name aks --service-endpoints Microsoft.KeyVault Microsoft.Storage
```

Now restrict the KeyVault so it's only accessible from the AKS subnet:

```
az keyvault network-rule add --vnet-name appnet --subnet aks -g labs-aks-apps --name <kv-name>

az keyvault update --default-action 'Deny' -g labs-aks-apps -n <kv-name>

az keyvault network-rule list -g labs-aks-apps --name <kv-name>
```

And add permission for the AKS Managed Identity to read secrets:

```
# print the identity ID:
az aks show -g labs-aks-apps -n aks06 --query addonProfiles.azureKeyvaultSecretsProvider.identity.clientId -o tsv

# add a policy to allow access to the identity:
az keyvault set-policy --secret-permissions get --spn '<identity-id>' -n <kv-name>
```

Check if you can read the secret with the CLI or the Portal again. It may take a few minutes for the rules to take effect, but now the secret should be blocked except for requests authenticated with the AKS Managed Identity, coming from the AKS subnet.

## Deploy app to AKS

We have the AKS cluster and all the other infrastructure created and wired up, so now we can deploy the app.

The Kubernetes model is fairly straightforward:

- [service.yaml](/labs/aks-apps/specs/asset-manager/service.yaml) - defines a LoadBalancer Service so we can access the app on a public IP address

- [deployment.yaml](/labs/aks-apps/specs/asset-manager/deployment.yaml) - the Deployment with a Pod spec which loads the KeyVault secret into a volume mount

- [secretProviderClass.yaml](/labs/aks-apps/specs/asset-manager/secretProviderClass.yaml) - the SecretProviderClass which makes the KeyVault secret available to mount

All the details are correct except for the placeholders in the secret provider. 

**Edit the file [secretProviderClass.yaml](/labs/aks-apps/specs/asset-manager/secretProviderClass.yaml) entering your own details for KeyVault name, AKS identity ID and your tenant**

Now you can connect to AKS and deploy the app:

```
az aks get-credentials -g labs-aks-apps -n aks06 --overwrite-existing

kubectl apply -f labs/aks-apps/specs/asset-manager
```

Wait for the Pod to be running:

```
kubectl get po --watch
```

Get the external IP address of the app:

```
kubectl get svc asset-manager-lb
```

Browse to the app - it should load the connection string from KeyVault, connect to Blob Storage and insert data, and you should see that on the page.

## Lab

But the Storage Account is still open to the Internet. Storage Accounts can't be deployed inside a VNet but they can be restricted. Fix the Storage Account so only Pods running n AKS have access.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG with this command to remove all the resources:

```
az group delete -y --no-wait -n labs-aks-apps
```