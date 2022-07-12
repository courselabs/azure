# Lab Solution

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