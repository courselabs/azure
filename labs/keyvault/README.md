# Azure Key Vault

Key Vault is a specialized storage service - it's for storing small pieces of sensitive data. You use it for user credentials, API keys, certificates and any other application configuration which shouldn't be visible in plain text. Key Vault data is encrypted at rest, you can set permissions for who can read values, and you can block access to the whole Key Vault so it's only available when you need to read the data.

## Reference

- [Key Vault docs](https://docs.microsoft.com/en-gb/azure/key-vault/)

- [`az keyvault` commands](https://docs.microsoft.com/en-us/cli/azure/keyvault?view=azure-cli-latest)

- [`az keyvault secret` commands](https://docs.microsoft.com/en-us/cli/azure/keyvault/secret?view=azure-cli-latest)

## Explore Key Vault in the Portal

Open the Portal and search to create a new Key Vault resource. Look at the main options:

- the premium pricing tier offers hardware encryption
- recovery options automatically retain deleted data as a safeguard
- access policies let you set low-level read and write permissions

We'll use the CLI to actually create a new Key Vault.

## Create a Key Vault with the CLI

Start with a new Resource Group, use your preferred region:

```
az group create -n labs-keyvault --tags courselabs=azure -l eastus
```

📋 Create a new Key Vault with the `keyvault create` command.

<details>
  <summary>Not sure how?</summary>

Start with the help:

```
az keyvault create --help
```

You need to specify the RG, region and a globally unique name:

```
az keyvault create -l eastus -g labs-keyvault -n <kv-name>
```

</details><br/>

> Creating the Key Vault will take a minute or two. While it runs, check the docs:

- what types of data can you store in a Key Vault?

## Manage Secrets in the Portal

Browse to your new Key Vault in the Portal.

Create a secret with the key `sql-password` which we could use to store credentials:

- does the workflow make sense?
- when you create the secret, how do you view it again?
- what happens if you need to update the secret?

> Secrets are versioned. You can view the current version, if you update the value then a new version is created and becomes the current verion. Old versions are still available.

## Manage Secrets in the CLI

Secrets have a unique identifier which contains the Key Vault name, secret name and version. It's shown in the Portal - copy the identifier of the latest version of your secret to the clipboard (it will look like this `https://sc-kv01-2003.vault.azure.net/secrets/sql-password/9989912ad43d4588971d9db2184990a6`).

You can show the secret data using just the ID:

```
az keyvault secret show --id <secret-id>
```

The response includes all the secret fields. You might want to retrieve just the secret value for automation.

📋 Add to the `secret show` command to display just the value in plain text.

<details>
  <summary>Not sure how?</summary>

Like other `az` commands you can add output and query parameters:

```
az keyvault secret show -o tsv --query "value" --id <secret-id>
```

</details><br/>

If you don't know the ID, you can get the latest version using the secret name:

```
az keyvault secret show --name sql-password  --vault-name <kv-name>
```

📋 Use other `secret` commands to update the value and print all the versions.

<details>
  <summary>Not sure how?</summary>

Check the commands available:

```
az keyvault secret --help
```

You use `secret set` to create or update a secret:

```
az keyvault secret set --name sql-password --value pw124123v4 --vault-name <kv-name>
```

And you can list all versions:

```
az keyvault secret list-versions --name sql-password --vault-name <kv-name>
```
</details><br/>

> Listing secret versions doesn't show the values, and it doesn't show which is the current version.

## Lab

Secrets are just one type of data which you can store in KeyVault. You can also generate and store encryption keys and TLS certificates. Use the CLI to create a self-signed certificate where the subject common name (CN) is `azure.courselabs.co` and which is valid for 6 months. Download the public and private keys for your new certificate.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources:

```
az group delete -y --no-wait -n labs-keyvault
```