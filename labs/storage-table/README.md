# Azure Table Storage

No-SQL database - part of a storage account; older technology but popular before Mongo so you'll see it in early Azure solutions. CosmosDB is a better option and has compatbility with table storage so there is a migration path.

Table -> rows. Rows have partition key (top-level grouping) and row key (item ID); data can be any structure

## Reference

- [Azure Table Storage overview](https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-overview)

- [Table storage design guidelines](https://docs.microsoft.com/en-us/azure/storage/tables/table-storage-design-guidelines)

- [OData query tutorial](https://www.odata.org/getting-started/basic-tutorial/#queryData)

- [`az storage table` commands](https://docs.microsoft.com/en-us/cli/azure/storage/table?view=azure-cli-latest)

## Create a table

Standard SA is fine:

```
az group create -n labs-storage-table  -l westeurope --tags courselabs=azure

az storage account create -g labs-storage-table  -l westeurope --sku Standard_LRS -n labsstoragetablees
```


Create the table:

```
az storage table create --help

az storage table create -n students --account-name labsstoragetablees
```

Browse to the portal - open the _tables_ view of the SA; not much to see. Open _Storage Browser_ instead and browse to the table.

Add some entities:

|Partition Key| Row Key | Properties|
|-|-|-|
|org1|1023|FirstName=x,LastName=y,Role=z|
|org1|1040|FirstName=a,LastName=b,Role=c|
|org2|aed1895|FullName=a b c,CountryCode=123|
|23124|stonemane||

Note that:

- entities can have different properties, no fixed schema per table
- partition key and row key can have different formats
- properties are not required

## Querying table storage with OData

Table storage provides an OData REST API:

```
# retrieve all students:
curl "https://labsstoragetablees.table.core.windows.net/students()"	
```

> You'll get a _Resource not found_ error - public access is not enabled

Generate a SAS token for the table. Expiry date needs to be in the format _YYYY-MM-DDTHH:MMZ_ e.g. _2022-08-10T11:44Z_

```
az storage table generate-sas --help 

# powershell
$expiry=$(Get-Date -Date (Get-Date).AddHours(1) -UFormat +%Y-%m-%dT%H:%MZ)

# zsh (use -d on Bash)
expiry=$(date -u -v+1H '+%Y-%m-%dT%H:%MZ')

az storage table generate-sas -n students --permissions r --expiry $expiry -o tsv --account-name labsstoragetablees
```

- SAS can be restricted to partition and row keys

Append the SAS token to the URL:

```
# all students:
curl "https://labsstoragetablees.table.core.windows.net/students()?se=2022-08-10T11%3A44Z&sp=r&sv=2019-02-02&tn=students&sig=RaAdiB13/otUPuZVWCG/dM50s5XbumAcThsO/9lMQP8%3D"

# student by org and ID:
curl "https://labsstoragetablees.table.core.windows.net/students(PartitionKey='org1',RowKey='1040')?se=2022-08-10T11%3A44Z&sp=r&sv=2019-02-02&tn=students&sig=RaAdiB13/otUPuZVWCG/dM50s5XbumAcThsO/9lMQP8%3D"
```

Response contains all properties - in XML :)

Ask for JSON instead:

```
curl -H 'Accept: application/json' "https://labsstoragetablees.table.core.windows.net/students(PartitionKey='org1',RowKey='1023')?se=2022-08-10T11%3A44Z&sp=r&sv=2019-02-02&tn=students&sig=RaAdiB13/otUPuZVWCG/dM50s5XbumAcThsO/9lMQP8%3D"

# without OData metadata:
curl -H 'Accept: application/json;odata=nometadata' "https://labsstoragetablees.table.core.windows.net/students(PartitionKey='org1',RowKey='1023')?se=2022-08-10T11%3A44Z&sp=r&sv=2019-02-02&tn=students&sig=RaAdiB13/otUPuZVWCG/dM50s5XbumAcThsO/9lMQP8%3D"
```

> Timestamp is automatically added when the row is inserted or updated

## Using table storage as a log sink

- [appsettings.json](src/fulfilment-processor/appsettings.json) - configuration to use Table Storage with Serilog;

Create a new table:

```
az storage table create -n FulfilmentLogs --account-name labsstoragetablees
```

Print the keys for the SA:

```
az storage account keys list -g labs-storage-table --account-name labsstoragetablees
```

A storage account connection string is in the format:

- `DefaultEndpointsProtocol=https;AccountName=<account-name>;AccountKey=<key1-or-key2>;EndpointSuffix=core.windows.net`

> You can also see the full connection string in the Portal, under _Access keys_

Update the appsettings json file with your connection details; then we'll deploy the app as an App Service WebJob (which is a background worker running in appservice):

```
az appservice plan create -g labs-storage-table  -n app-plan-01 --sku B1 --number-of-workers 1

az webapp create -g labs-storage-table --plan app-plan-01 --runtime dotnet:6 -n labs-storage-table-es # <dns-unique-name>

az webapp config set --always-on true   -g labs-storage-table -n labs-storage-table-es # <dns-unique-name>
```

Now configure & deploy the app:

```
az webapp config appsettings set --settings Serilog__WriteTo__0__Args__connectionString='<connection-string>' -g labs-storage-table -n labs-storage-table-es # <dns-unique-name> 

az webapp config appsettings set --settings Serilog__WriteTo__0__Args__connectionString='DefaultEndpointsProtocol=https;AccountName=labsstoragetablees;AccountKey=vC1xD9I8hUjJYF7OnpPS4R5gAfON/bDaExiLy+9kEYLxEJG8PmMsLFNXpPB9IFrrm6jnw38swxMh+AStw8Oepw==;EndpointSuffix=core.windows.net' -g labs-storage-table -n labs-storage-table-es # <dns-unique-name>

az webapp deployment source config-zip -g labs-storage-table  --src src/fulfilment-processor/deploy.zip -n labs-storage-table-es
```

Open the portal; when deployment completes you will see a _WebJob_ under the app service app.

Now check the new FulfilmentLogs table - you'll see lots of entries

- how are the partitionkey and rowkey constructed?


## Lab

query the fulfilment logs to find error events