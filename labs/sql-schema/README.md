# Deploying database schemas

Dacpac & SQL database project - repeatable, source controlled, versioned

## Reference

- [Azure SQL docs](https://docs.microsoft.com/en-gb/azure/azure-sql/)

- [`az sql server` commands](https://docs.microsoft.com/en-us/cli/azure/sql/server?view=azure-cli-latest)

- [`az sql db` commands](https://docs.microsoft.com/en-us/cli/azure/sql/db?view=azure-cli-latest)


## Create a SQL Server 

Start by creating a SQL Server we can use for a new database:

```
# set some variables - PowerShell:
$location='westeurope'
$rg='labs-sql-schema'
$server='labs-sql-schema-es'
$database='assets-db'

# OR Bash:
location='westeurope'
rg='labs-sql-schema'
server='labs-sql-schema-es'
database='assets-db'


# create RG, server & database:
az group create -n $rg  -l $location --tags courselabs=azure

az sql server create -g $rg -l $location -n $server -u sqladmin -p <admin-password>
```

Browse to your server in the Portal:

- click on the _Import Database_ option from the top menu
- explore the configuration choices
- what inputs do you need and what output would you expect?

> You can import a database from a BACPAC (database backup) or DACPAC (database definition) file. 

The file you import needs to be stored in Azure, so we'll start by uploading a DACPAC file.

## Upload the DACPAC file

You can upload files to an Azure Storage Account. We'll cover that in more detail, but this will get you started:

```
# create the storage account - you need a globally unique name with only lowercase characters:
az storage account create  -g $rg -l $location --sku Standard_LRS -n <storage-account-name>

# create a container where we can upload files:
az storage container create -n dacpacs --account-name <storage-account-name>

# upload a local file to the container - Azure calls files BLOBs (Binary Larg OBjects):
az storage blob upload -f ./labs/sql-schema/assets-db/Assets.Database.dacpac -c dacpacs -n assets-db.dacpac --account-name <storage-account-name>
```

📋 Open the Resource Group in the Portal and find the details of your uploaded Dacpac. Can you download it from the URL?

<details>
  <summary>Not sure how?</summary>

Refresh the Resource Group and you'll see the Storage Account listed. Open that resource:

- under the _Data Storage_ left menu you'll see _Containers_
- open that and you'll see all the blob containers listed
- open the _dacpacs_ container and you'll see the uploaded file
- click the file and you'll see the details - including the URL

</details><br/>

Blob storage can be used as a public file share. Your blob will have a URL like `https://labssqlschemaes.blob.core.windows.net/dacpacs/assets-db.dacpac` - the first part of the address is the Storage Account name, which is why it needs to be globally unique.

You can't download from that address though - by default new blob containers are set to private, so they can only be accessed within Azure. That's fine for what we want to do.

## Import the Dacpac into a new database

In the CLI you need to create a new database first, then you can import Dacpac into that database:

```
az sql db create -g $rg -n $database -s $server
```

> While that's running you can continue, getting the details ready for the import

Database imports are a quick way to create a new database from a backup - it's a simple process to export an on-prem database to a Bacpac file and create it in Azure, complete with all the data.

Bacpacs and Dacpacs use the same file format, so we can use a Dacpac file which has the database schema and some reference data. If you're not familiar with SQL Projects, you'll find the setup in the `labs/sql-schema/assets-db/src` folder.

📋 Create a database from the Dacpac file you uploaded using the `sql db import` command.

<details>
  <summary>Not sure how?</summary>

Check the help text:

```
az sql db import --help
```

The help text only talks about Bacpacs - but remember Dacpacs use the same format, so they're valid too. You need to specify:

- admin credentials for the SQL Server
- URL for the file to import from the Storage Account
- storage account key
- name of the new database and the server to use

You need to get the access key for your blob storage - the help text gives you an example of to get a Shared Access Key (SAS):

```
az storage blob generate-sas  -c dacpacs -n assets-db.dacpac --permissions r --expiry 2030-01-01T00:00:00Z --account-name <storage-account-name>
```

You'll get a key in the output which you can plug into the `import` command:

```
az sql db import -s $server -n $database -g $rg --storage-key-type SharedAccessKey -u sqladmin -p <server-password>  --storage-key <sas-key> --storage-uri <dacpac-url>
```

</details><br/>

There are a few pieces you need to put together here, but if there are any issues the CLI output should help you to fix them up.

Creating the database will take a little while

## Use the new database

Open the database in the Portal and open the _Query Editor_ blade - this lets you connect to the database and run SQL commands from the browser. Log in with the admin credentials you set in the `import` command and check the schema has been deployed.

📋 The Dacpac includes some reference data which gets inserted in the import. Can you find the postcode of the UK location?

<details>
  <summary>Not sure how?</summary>

You'll get a connection error to start with because public IP addresses are blocked. The Portal will let you easily add your current IP address to the allowlist.

Then


## Lab

Use CLI to delete the SQL database. When the database is gone the SQL Server still exists - can you retrieve the data from your deleted database? Now delete the resource group, does the SQL Server still exist?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

If you didn't finish the lab, you can delete the RG with this command to remove all the resources:

```
az group delete -y -n labs-sql
```