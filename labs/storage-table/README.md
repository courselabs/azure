# Azure Table Storage

Table storage is a simple, scalable database which you can host in an Azure Storage Account. It takes a no-SQL approach, so you need to use the dedicated libraries in your code to read and write data. It's an older part of the storage stack in Azure, but it was around before alternatives like Mongo and so you'll see it in early Azure solutions. CosmosDB is a better option but it has compatbility with table storage so there is a migration path.

In this lab we'll use table storage in a simple app and see how data is stored and accessed.be any structure

## Reference

- [Azure Table Storage overview](https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-overview)

- [Table storage design guidelines](https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-design-guidelines)

- [OData query tutorial](https://www.odata.org/getting-started/basic-tutorial/#queryData)

- [`az storage table` commands](https://docs.microsoft.com/en-us/cli/azure/storage/table?view=azure-cli-latest)

## Create a  Table

Table Storage is a feature of Storage Accounts, so we'll start by creating a Resource Group and a Storage Account:

```
az group create -n labs-storage-table --tags courselabs=azure -l westeurope

az storage account create -g labs-storage-table --sku Standard_LRS -l westeurope -n <sa-name> 
```

📋 Use the `storage table create` command to create a table in the SA called `students`.

<details>
  <summary>Not sure how?</summary>

```
az storage table create --help
```

The only mandatory parameters are the table name and the SA name:

```
az storage table create -n students --account-name <sa-name>
```

</details><br/>

> The output just says the table has been created. Empty tables don't cost anything - you only pay when there's data in the table.

Browse to the Portal - open the _tables_ view of the Storage Account. There's not much to see. Open _Storage Browser_ instead and browse to the table.

Table storage uses different terminology to a SQL database:

- _entities_ are the data items, like rows in SQL or objects in Mongo
- _partition key_ is part of the entity's unique ID, a grouping which is used to determine where the data is stored
- _row key_ is the unique part of the entity's ID

Use the storage browser to add some entities:

|Partition Key| Row Key | Properties|
|-|-|-|
|org1|1023|FirstName=x,LastName=y,Role=z|
|org1|1040|FirstName=a,LastName=b,Role=c|
|org2|aed1895|FullName=a b c,CountryCode=123|
|23124|stonemane||

Note that:

- entities can have different properties, tables do not have a fixed schema
- partition key and row key can have different formats, you can mix strings and integers
- properties are not required, you can have an empty entity

## Querying Table Storage with OData

Table storage provides an OData REST API, so you can query your data with curl.

Print out the table storage domain name for your account:

```
az storage account show --query 'primaryEndpoints.table' -o tsv -n <sa-name>
```

You can add the table name to the end of that URL, with an empty query `()` to retrieve all entities:

```
curl '<table-storage-url>/students()'
```

> You'll get a _Resource not found_ error. OData is supported, but public access is not enabled

📋 Generate a SAS token for the account which you can use to read the `students` table. The token should be valid for 2 hours. 

<details>
  <summary>Not sure how?</summary>

You can do this in the Portal - but the token needs to be created at the Storage Account level, not the table. Open the _Shared access signature_ blade and complete the fields for table storage.

Or with the CLI, you can get a token just for one table, and use some fancy scripts to generate the expiry date in the right format:

```
az storage table generate-sas --help 

# PowerShell:
$expiry=$(Get-Date -Date (Get-Date).AddHours(2) -UFormat +%Y-%m-%dT%H:%MZ)

# OR zsh: 
expiry=$(date -u -v+2H '+%Y-%m-%dT%H:%MZ')

# OR manually if none of the above work :)
expiry='2022-12-31T23:59Z'

az storage table generate-sas -n students --permissions r --expiry $expiry -o tsv --account-name <sa-name>
```

</details><br/>

> Using the CLI you can generate a fine-grained token which is restricted to a range of partition and row keys

Try the OData query again, but now append the SAS token to the URL. Your full URL will look like this - but your own domain name and token will be different: `https://labsstoragetablees.table.core.windows.net/students()?se=2022-10-27T19%3A59Z&sp=r&sv=2019-02-02&tn=students&sig=DM6tZRoUcCzO0EVepJF6KF%2BJeMGktbD5vEjvbOqNUAw%3D`

**Wrap your OData URLs with double-quotes!**

```
# this returns all students:
curl "<table-url>/students()?<sas-token>"

# this filters students by both keys:
curl "<table-url>/students(PartitionKey='org1',RowKey='1040')?<sas-token>"
```

The request contains the query in the brackets after the table name. The response contains all entities that match the query, with all their properties.

XML is the default response format, but you can request JSON with an HTTP header:

```
# ask for a JSON response:
curl -H 'Accept: application/json' "<table-url>/students(PartitionKey='org1',RowKey='1023')?<sas-token>"

# without OData metadata:
curl -H 'Accept: application/json;odata=nometadata' "<table-url>/students(PartitionKey='org1',RowKey='1023')?<sas-token>"
```

> You'll see in the data that a _Timestamp_ field is automatically set when the row is inserted or updated

OData is not that commonly used, but it's there as a feature in Table Storage and it can be a good option for sharing data without having to build your own REST API.

More typically you'll use a client library to read and write data in code.

## Using table storage as a log sink

We'll run a simple .NET application which uses a logging library called [Serilog](https://serilog.net). Serilog can write log data to different types of storage, and Table Storage is one of the options.

We need to create the table first and get the connection details before we can run the app.

📋 Generate a new table called _FulfilmentLogs_ in your Storage Account, and print the connection string for apps to authenticate. 

<details>
  <summary>Not sure how?</summary>

Create the new table:

```
az storage table create -n FulfilmentLogs --account-name <sa-name>
```

Print the connection string for the SA:

```
az storage account show-connection-string -g labs-storage-table -n <sa-name>
```

</details><br/>

You can also see the connection string in the Portal under _Access keys_. A full storage account connection string is in the format:

- `DefaultEndpointsProtocol=https;AccountName=<sa-name>;AccountKey=<key1-or-key2>;EndpointSuffix=core.windows.net;BlobEndpoint=https://<sa-name>.blob.core.windows.net/;FileEndpoint=https://<sa-name>.file.core.windows.net/;QueueEndpoint=https://<sa-name>.queue.core.windows.net/;TableEndpoint=https://<sa-name>.table.core.windows.net/` 

We'll run the app locally and see how logs get written to the cloud.

> You'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed to run the app

Start by updating the app configuration file:

- [appsettings.json](/src/fulfilment-processor/appsettings.json) - open the file and replace the value `<sa-connection-string>` with your actual connection string

Run the app:

```
dotnet run --project src/fulfilment-processor
```

> You won't see any output, all the logs are being written to Table Storage

Let the app run for a few minutes, then exit with `Ctrl-C` or `Cmd-C`.

Open the Portal and check the `FulfilmentLogs` table in the Storage Browser - you'll see lots of entries. How are the PartitionKey and RowKey constructed?

## Lab

Can you query the fulfilment log entries to find just the error events? You can do this in the browser or with OData. What do you think you would need to do to fix the issue?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG to clean up:

```
az group delete -y -n labs-storage-table --no-wait
```