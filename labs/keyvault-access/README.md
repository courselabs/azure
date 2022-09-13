# Securing Key Vault Access

## Reference


- [Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)

- [`az vm identity assign` command](https://docs.microsoft.com/en-us/cli/azure/vm/identity?view=azure-cli-latest#az-vm-identity-assign)

- [`az keyvault network-rule` commands](https://docs.microsoft.com/en-us/cli/azure/keyvault/network-rule?view=azure-cli-latest)

## Create RG, KeyVault and Secret

```
az group create -n labs-keyvault-access --tags courselabs=azure -l eastus

az keyvault create -l eastus -g labs-keyvault-access -n <kv-name>

az keyvault secret set --name secret01 --value azure-labs --vault-name <kv-name>
```


Check you can read the secret from your own machine:

```
az keyvault secret show --name secret01 -o tsv --query "value" --vault-name <kv-name>
```


## Restrict Access to VNet

Create VNet and subnet:

```
az network vnet create -g labs-keyvault-access -n vnet1 --address-prefix "10.10.0.0/16"

az network vnet subnet create -g labs-keyvault-access --vnet-name vnet1 -n subnet1 --address-prefix "10.10.1.0/24"
```

Add a network rule to restrict access:

```
# this will show an error:
az keyvault network-rule add -g labs-keyvault-access --vnet-name vnet1 --subnet subnet1 --name <kv-name>
```

First you need to update the subnet to allow a service endpoint:

```
az network vnet subnet update -g labs-keyvault-access --vnet-name vnet1 -n subnet1 --service-endpoints 'Microsoft.KeyVault'
```

Now you can add the network rule:

```
az keyvault network-rule add -g labs-keyvault-access --vnet-name vnet1 --subnet subnet1 -n <kv-name>
```

> Open your VNet in the Portal. In the _Service Endpoints_ an _Subnets_ tabs you'll see KeyVault listed.

Try and print the secret value from your local machine again. Your machine is not in the VNet - do you get an access error?

```
# you can stil print the secret:
az keyvault secret show --name secret01 -o tsv --query "value" --vault-name labs-keyvault-access-es
```

> Open the KeyVault in the Portal and browse to _Networking_. You'll see the default value _Allow public access from all networks_ is selected - adding a network rule doesn't change this.

```
az keyvault update --default-action Deny -g labs-keyvault-access -n labs-keyvault-access-es 
```

Now try to print the secret:

```
# this will fail:
az keyvault secret show --name secret01 -o tsv --query "value" --vault-name labs-keyvault-access-es
```


## Create a VM with access to the KeyVault

```
az vm create -g labs-keyvault-access -n vm01 --image UbuntuLTS --vnet-name vnet1 --subnet subnet1
```

Connect to the VM and run the Python script to access the KeyVault:

```
ssh <vm-public-ip>

sudo apt update && sudo apt install -y python3-pip

pip3 install --upgrade pip
pip3 install azure-keyvault-secrets
pip3 install azure.identity

curl -o read-secret.py https://gist.githubusercontent.com/sixeyed/642c9f85dd3d7dc689c786d143080eb9/raw/24ed917ff7bfc2b1070990a47767b43ed8ab0d88/read-secret.py

# this will fail:
python3 read-secret.py
```

You'll see an authentication error. The VM is inside the subnet which has access to the KeyVault **but** to consume a secret you still need to use an authenticated identity.

In a new terminal, add a system-generated managed identity to the VM:

```
# the output from this command contains the managed identity ID:
az vm identity assign -n vm01 -g labs-keyvault-access

# give the identity access to read secrets:
az keyvault set-policy  --object-id "<systemAssignedIdentity>" --secret-permissions get -n <kv-name>


az keyvault set-policy  --object-id 68ddf972-075c-4fa8-8725-c6cd49ded7ed --secret-permissions get -n labs-keyvault-access-es
```

Now repeat the Python script in your VM shell session:

```
python3 read-secret.py
```

> You'll see the secret value; the VM authenticates with Managed Identity so there's no credentials to supply for accessing KeyVault.

## Lab


A common use for a Key Vault is for automated deployments. You'll create the Key Vault in your pipeline and use it to store credentials you need for other services - maybe generating a random password for a SQL Server admin account.

In that scenario you only want the Key Vault to be accessible while the pipeline is running. How can you lock down a Key Vault so it can't be used when the pipeline has finished?


---


A Key Vault can be set with public network access disabled, which means the Key Vault can't be accessed from outside of Azure, not even with the command line.

You can disable access to lock the Key Vault down:

```
az keyvault update --public-network-access Disabled -g labs-keyvault -n <kv-name>
```

Now try to show a secret and you'll get an error:

```
az keyvault secret show --name sql-password  --vault-name <kv-name>
```

The error message tells you the Key Vault is not accessible over the network:

*Connection is not an approved private link and caller was ignored because bypass is not set to 'AzureServices' and PublicNetworkAccess is set to 'Disabled'. Vault: es-kv-001;location=eastus*