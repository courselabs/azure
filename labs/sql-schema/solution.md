# Lab Solution

Using the Portal:

- open the Azure SQL database
- the _Export_ option is on the top menu
- enter a file name for the Bacpac
- select the Azure Storage Account you created in the lab
- enter your SQL authentication details

Or with the CLI:

```
# print the help:
az sql db export --help

# generate a SAS token to write files:
az storage blob generate-sas  -c databases -n assets-db.bacpac --permissions w --expiry 2030-01-01T00:00:00Z --account-name <storage-account-name>

# export the Bacpac:
az sql db export -s $server -n $database -g $rg -p <sql-password> -u sqladmin --storage-key-type SharedAccessKey --storage-key <sas-token> --storage-uri https://<storage-account-name>.blob.core.windows.net/databases/assets-db.bacpac
```

Now your Bacpac is updated in Azure Storage you can use it in an import command for a different Azure SQL instance.