# Deploying database schemas

Empty SQL databases are fine if your app uses an ORM and can create its own schema - but not every app can do that. Microsoft has a packaging format for database schemas which you upload to Azure and deploy to an Azure SQL instance.

In this lab we'll take an existing database backup file and deploy it to a new database so it's ready for an app to use.

## Reference

- [SQL Server projects in VS Code](https://learn.microsoft.com/en-us/sql/azure-data-studio/extensions/sql-database-project-extension?view=sql-server-ver16)

- [Data tier applications - Bacpac & Dacpac files](https://learn.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications?view=sql-server-ver16)

- [`az sql db` commands](https://docs.microsoft.com/en-us/cli/azure/sql/db?view=azure-cli-latest)

## Create a SQL Server 

Start by creating a SQL Server we can use for a new database. We'll be using the same parameters a few times, so start by storing some variables.

```
# set some variables - PowerShell:
$location='westeurope'
$rg='labs-sql-schema'
$server='<unique-server-dns-name>'
$database='assets-db'

# OR Bash:
location='westeurope'
rg='labs-sql-schema'
server='<unique-server-dns-name>'
database='assets-db'

# create RG, server & database:
az group create -n $rg  -l $location --tags courselabs=azure

az sql server create -g $rg -l $location -n $server -u sqladmin -p <admin-password>
```

Browse to your server in the Portal:

- click on the _Import Database_ option from the top menu
- explore the configuration choices
- what inputs do you need and what output would you expect?

> You can import a database from a Bacpac (database backup) file. 

The file you import needs to be stored in Azure, so we'll start by uploading a Bacpac file.

## Upload the Bacpac file

You can upload files to an Azure Storage Account. 

We'll have dedicated labs covering storage in more detail, but this will get you started:

```
# create the storage account - you need a globally unique name with only lowercase characters:
az storage account create  -g $rg -l $location --sku Standard_LRS -n <storage-account-name>

# create a container where we can upload files:
az storage container create -n databases --account-name <storage-account-name>

# upload a local file to the container - Azure calls files BLOBs (Binary Large OBjects):
az storage blob upload -f ./labs/sql-schema/assets-db.bacpac -c databases -n assets-db.bacpac --account-name <storage-account-name>
```

📋 Open the Resource Group in the Portal and find the details of your uploaded Bacpac. Can you download it from the URL?

<details>
  <summary>Not sure how?</summary>

Refresh the Resource Group and you'll see the Storage Account listed. Open that resource:

- under the _Data Storage_ left menu you'll see _Containers_
- open that and you'll see all the blob containers listed
- open the _databases_ container and you'll see the uploaded file
- click the file and you'll see the details - including the URL

</details><br/>

Blob storage can be used as a public file share. Your blob will have a URL like `https://labssqlschemaes.blob.core.windows.net/databases/assets-db.bacpac` - the first part of the address is the Storage Account name, which is why it needs to be globally unique.

You can't download from that address though. By default new blob containers are set to private, so they can only be accessed within Azure. That's fine for what we want to do.

## Import the Bacpac into a new database

In the CLI you need to create a new database first and grant access from your local machine, then you can import the Bacpac you uploaded into that database:

```
az sql db create -g $rg -n $database -s $server
```

> While that's running you can continue, getting the details ready for the import.

Database imports are a quick way to create a new database from a backup - it's a simple process to export an on-prem database to a Bacpac file, upload it to Azure and import it into an Azure SQL database, complete with all the data.

- Bacpacs are an export of a database schema and the data
- Dacpacs are a model of a database schema - without the data - you can't import from a Dacpac

📋 Create a database from the Bacpac file you uploaded using the `sql db import` command.

<details>
  <summary>Not sure how?</summary>

Check the help text:

```
az sql db import --help

# allow access for internal Azure services:
az sql server firewall-rule create -g $rg -s $server -n azure --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

# find your public IP address (or browse to https://www.whatsmyip.org)
curl ifconfig.me

az sql server firewall-rule create -g $rg -s $server -n client --start-ip-address <ip-address> --end-ip-address <ip-address> 
```

You need to specify:

- admin credentials for the SQL Server
- URL for the file to import from the Storage Account
- storage account key
- name of the new database and the server to use

You need to get the access key for your blob storage. The help text gives you an example of to get a Shared Access Key (SAS), which is an authentication token to access the file:

```
# generate and print an authentication token:
az storage blob generate-sas  -c databases -n assets-db.bacpac --permissions r --expiry 2030-01-01T00:00:00Z --account-name <storage-account-name>
```

You'll get a key in the output which you can plug into the `import` command:

```
az sql db import -s $server -n $database -g $rg --storage-key-type SharedAccessKey -u sqladmin -p <server-password> --storage-uri <bacpac-url> --storage-key <sas-key>
```

</details><br/>

There are a few pieces you need to put together here, but if there are any issues the CLI output should help you to fix them up.

> Creating the database [can take a long time](https://docs.microsoft.com/en-US/azure/azure-sql/database/database-import-export-hang?view=azuresql) - you can check the progress in the Portal, opening the _Import/Export history_ tab from the SQL Server blade.

## Use the new database

Open the database in the Portal and open the _Query Editor_ blade - this lets you connect to the database and run SQL commands from the browser. Log in with the admin credentials you set in the `import` command and check the schema has been deployed.

📋 The Bacpac includes some reference data which gets inserted in the import. Can you find the postcode of the UK location?

<details>
  <summary>Not sure how?</summary>

The query editor window has an object explorer on the left hand side - you can navigate the schema here and find the table and column names.

Then it's just standard SQL statements you can run inside the editor:

```
SELECT * FROM Locations

SELECT PostalCode FROM Locations WHERE Country='UK'
```

</details><br/>


## Lab

Insert some data into the assets table:

```
INSERT INTO [dbo].[Assets] (AssetTypeId, LocationId, AssetDescription)
VALUES (1, 1, 'Elton''s MacBook Air')

INSERT INTO [dbo].[Assets] (AssetTypeId, LocationId, AssetDescription)
VALUES (2, 2, 'Elton''s Mac Studio')

INSERT INTO [dbo].[Assets] (AssetTypeId, LocationId, AssetDescription)
VALUES (3, 2, 'Elton''s iPhone')
```

This is additional data, not present in the original Bacpac. Export a Bacpac from the Azure database. How would you use that file to recreate the data in another instance.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG with this command to remove all the resources:

```
az group delete -y -n labs-sql-schema
```