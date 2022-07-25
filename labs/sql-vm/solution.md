# Lab Solution

Print the help for the delete command:

```
az sql db delete --help
```

You need to specify the database name, server name and Resource Group, e.g:

```
az sql db delete --name db01 --resource-group labs-sqlserver --server <server-name>
```

> You'll be asked for confirmation.

When the command completes, browse to the SQL Server instance in the Portal and open the _Deleted databases_ section. 

You can [restore a deleted database in the Portal](https://docs.microsoft.com/en-us/azure/azure-sql/database/recovery-using-backups#deleted-database-restore-by-using-the-azure-portal), but it will take a few minutes before newly-deleted databases show up.

Deleting the Resource Group will delete the SQL Server:

```
az group delete -n labs-sqlserver -y
```

That removes any databases and backups, and you can no longer restore deleted databases.