# Securing Key Vault Access

Key Vaults are full of sensitive data so you need to secure access to them. You can use Azure AD to restrict access, which limits what users can do in the Portal and the `az` command. You also need to secure the Key Vault internally, to ensure that only the components which need to read the data actually have access to the vault.

In this lab we'll see how to restrict Key Vault access to virtual networks and Azure identities.

## Reference

- [Key Vault best practices](https://learn.microsoft.com/en-us/azure/key-vault/general/best-practices)

- [Managed Identities](https://docs.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview)

- [`az vm identity assign` command](https://docs.microsoft.com/en-us/cli/azure/vm/identity?view=azure-cli-latest#az-vm-identity-assign)

- [`az keyvault network-rule` commands](https://docs.microsoft.com/en-us/cli/azure/keyvault/network-rule?view=azure-cli-latest)

## Create RG, KeyVault and Secret

Start by creating a KeyVault in a new Resource Group:

```
az group create -n labs-keyvault-access --tags courselabs=azure -l eastus

az keyvault create -l eastus -g labs-keyvault-access -n <kv-name>
```

Create a secret in the vault and confirm you can read it back again from your own machine:

```
az keyvault secret set --name secret01 --value azure-labs --vault-name <kv-name>

az keyvault secret show --name secret01 -o tsv --query "value" --vault-name <kv-name>
```

You created the account so you have all permissions. Open the Key Vault in the Portal and you can see how the permissions are defined in the _Access policies_ tab.

📋 Can you give me access to your Key Vault, so I can list and read secrets using my account `elton@sixeyed.com`?

<details>
  <summary>Not sure how?</summary>

You can add a new access policy and select the permissions you need, then you need to select a _principal_ to grant the permissions to.

Enter my email and you'll see no results found. The list of principals you can use is limited to your own Azure AD account, an my identity is in a different AD account.

If you wanted to give me access, you'd need to add me as an external ID in your Azure Active Directory.

Don't do that, I don't really want access :)

</details><br/>

Azure talks about _principals_ when you're applying security. That's a general term which could refer to:

- a user with a Microsoft Account
- a group of users
- a sytem identity used by an Azure resource
- a managed identity for a resource which is managed by Azure

You need to consider all those, because you don't want any unauthorized access to your secrets. 

Before a principal can authenticate, they need network access to the Key Vault, which you can also restrict.

## Restrict Access to VNet

We'll create a Virtual Network and run in a VM in the network. We'll set up KeyVault so it can only be used by the VM.

Start with the VNet and subnet:

```
az network vnet create -g labs-keyvault-access -n vnet1 --address-prefix "10.10.0.0/16"

az network vnet subnet create -g labs-keyvault-access --vnet-name vnet1 -n subnet1 --address-prefix "10.10.1.0/24"
```

📋 Use the `keyvault network-rule add` command to give access to the KeyVault from any services running in the subnet.

<details>
  <summary>Not sure how?</summary>

Check the help:

```
az keyvault network-rule add --help
```

Try to add the subnet:

```
# this will show an error:
az keyvault network-rule add -g labs-keyvault-access --vnet-name vnet1 --subnet subnet1 --name <kv-name>
```

Other services aren't allowed to route traffic to subnets unless you explicitly allow them with a _service endpoint_. This sets the subnet so Key Vault resources are allowed into the subnet:

```
az network vnet subnet update -g labs-keyvault-access --vnet-name vnet1 -n subnet1 --service-endpoints 'Microsoft.KeyVault'
```

Now you can add the network rule:

```
az keyvault network-rule add -g labs-keyvault-access --vnet-name vnet1 --subnet subnet1 -n <kv-name>
```

</details><br/>

Any Azure resources which need access to a subnet have to have a [service endpoint](https://learn.microsoft.com/en-us/azure/virtual-network/virtual-network-service-endpoints-overview) set up, but this only needs to be done once for each service type that's going to use the subnet.

> Open your VNet in the Portal. In the _Service Endpoints_ an _Subnets_ tabs you'll see KeyVault listed.

Try and print the secret value from your local machine again. Your machine is not in the VNet - do you get an access error?

```
# you can stil print the secret:
az keyvault secret show --name secret01 -o tsv --query "value" --vault-name <kv-name>
```

> Open the KeyVault in the Portal and browse to _Networking_. You'll see the default value _Allow public access from all networks_ is selected - adding a network rule doesn't change this.

Update the Key Vault so access is denied unless there's a network rule to allow it:

```
az keyvault update --default-action Deny -g labs-keyvault-access -n <kv-name>
```

Now try to print the secret:

```
# this will fail:
az keyvault secret show --name secret01 -o tsv --query "value" --vault-name <kv-name>
```

Your Key Vault is locked down now, so only resources in the subnet can use it.

## Create a VM with access to the KeyVault

Now we'll create a VM in the subnet to prove we can still access the secrets. This is a simple Ubuntu Server VM which we're creating, and we'll use two scripts:

- [scripts/setup.sh](/labs/keyvault-access/scripts/setup.sh) - installs Python and the libraries we need to use Key Vault
- [scripts/read-secret.py](/labs/keyvault-access/scripts/read-secret.py) - the Python script we'll run on the machine to test access to the KeyVault

Create the VM with the setup script:

```
az vm create -g labs-keyvault-access -n vm01 --image UbuntuLTS --vnet-name vnet1 --subnet subnet1 --custom-data @labs/keyvault-access/scripts/setup.sh
```

When the VM is ready, connect and try the Python script:

```
ssh <vm-public-ip>

# download the Python script:
curl -o read-secret.py https://raw.githubusercontent.com/courselabs/azure/main/labs/keyvault-access/scripts/read-secret.py

# and run it - this will fail:
python3 read-secret.py
```

You'll see an authentication error. The VM is inside the subnet which has access to the KeyVault **but** to consume a secret you still need to use an authenticated Azure principal. We'll use a managed identity for that.

In a new terminal add a system-generated managed identity to the VM:

```
# the output from this command contains the managed identity ID:
az vm identity assign -n vm01 -g labs-keyvault-access

# give the identity access to read secrets:
az keyvault set-policy --secret-permissions get --object-id <systemAssignedIdentity>  -n <kv-name>
```

Now repeat the Python script in your VM shell session:

```
python3 read-secret.py
```

> You'll see the secret value. the VM authenticates with Managed Identity so there's no credential to supply for accessing KeyVault.

Managed identities are only used within Azure services to authenticate with other Azure services. Check the access policies for your KeyVault in the Portal and you'll see how it's set up.

## Lab

Key Vault has a soft-delete policy by default - if you delete a secret by accident, then you can restore it. Try that with the `secret01` secret, delete it and then try to recreate it. What do you need to do to make that work, so in the VM your Python script prints the new value of the secret?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources:

```
az group delete -y --no-wait -n labs-keyvault-access
```