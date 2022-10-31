# Cosmos DB - Table API

The Cosmos DB Table API is a straight replacment for Azure Table Storage. It offers an easy migration paths for older apps built with Table Storage. You can use Cosmose Table API without changing those apps, and move to the modern storage option with the scale and capabilities of Cosmos.

In this lab we'll explore the Table API, then run an application which writes to Table Storage and switch it to using CosmosDB with just a config change.

## Reference

- [Cosmos Capacity Calculator](https://cosmos.azure.com/capacitycalculator/)

- [Migrate Table Storage to Cosmos](https://learn.microsoft.com/en-us/azure/cosmos-db/table/import) - 3rd-party tool

- [Querying Cosmos Table API](https://learn.microsoft.com/en-us/azure/cosmos-db/table/tutorial-query)

- [`az cosmosdb table` commands](https://docs.microsoft.com/en-us/cli/azure/cosmosdb/table?view=azure-cli-latest)


## Create a Cosmos DB Table database

Create a new resource in the Portal - search for "cosmos", create a new CosmosDB and choose the Table API:

- the options are the same as the SQL API
- the underlying database engine is the document DB for both

Create a Cosmos DB account with the CLI:

```
az group create -n labs-cosmos-table  --tags courselabs=azure -l westeurope

az cosmosdb create --help

az cosmosdb create -g labs-cosmos-table --enable-public-network --kind GlobalDocumentDB --capabilities EnableTable -n <cosmos-db-name>
```

> We're using the same _GlobalDocumentDB_ kind we use for the NoSQL API, but the additional capability flag enables the Table API

Open the new database in the Portal. There are some different options from the other flavours of Cosmos:

- there are no _Collections_ or _Containers_ - the Table API just has one level in the structure: Account -> Table(s)
- the _Integrations_ sectionwhere you can add an Azure Function which will be triggered when data changes

📋 Create a database called `FulfilmentLogs` using the Table API

<details>
  <summary>Not sure how?</summary>

We want the `cosmosdb table create` command:

```
az cosmosdb table create --help

az cosmosdb table create --name FulfilmentLogs -g labs-cosmos-table --account-name <cosmos-db-name>
```

</details><br/>

Open _Data Explorer_ in the Portal for your CosmosDB. You'll see the new table, but expand it and there are no entities.

## Using Cosmos Table API as a log sink

We used an app in the [Table Storage lab](/labs/storage-table/README.md) which wrote log entries to Table Storage. We'll deploy that app to Azure using Table Storage to start with, then switch over to Cosmos with just a config change.

The app's logging configuration is set here:

- [appsettings.json](/src/fulfilment-processor/appsettings.json) - the Storage Account connection string is a placeholder; we'll set the real value as an application setting in Azure

Start by creating a Storage Account and Table which will be the "legacy" data store:

```
az storage account create -g labs-cosmos-table --sku Standard_LRS -l westeurope -n <sa-name>

az storage table create -n FulfilmentLogs --account-name <sa-name>
```

Now we can go on to deploy the app which will write logs to the table.

📋 Create a new App Service Plan on the Basic SKU, and .NET 6 Web App in the lab RG. 

<details>
  <summary>Not sure how?</summary>

```
az appservice plan create -g labs-cosmos-table -n app-plan-01 --sku B1 --number-of-workers 1

az webapp create -g labs-cosmos-table --plan app-plan-01 --runtime dotnet:6 -n <web-app-name>
```

</details><br/>

Web Apps are intended for HTTP applications, but they are also able to run background processes in the same hosting environment. Open the Web App in the Portal and check the _WebJobs_ page - there are none right now, but we can upload a web job to run our demo app in the background.

First we need to set the Always On flag so the hosting environment doesn't shut down when it sees there's no website to run:

```
az webapp config set --always-on true   -g labs-cosmos-table -n <web-app-name>
```

Next grab the connection string for your Storage Table and set is as a configuration item in the Web App:

```
az storage account show-connection-string -g labs-cosmos-table --query connectionString -o tsv -n <sa-name>

az webapp config appsettings set --settings Serilog__WriteTo__0__Args__connectionString='<sa-connection-string>' -g labs-cosmos-table -n <web-app-name> 
```

Now deploy the app - web job deployment doesn't fit any of the normal App Service deployment options, you need to upload a ZIP file with your compiled application already in it:

```
az webapp deployment source config-zip -g labs-cosmos-table --src src/fulfilment-processor/deploy.zip -n <web-app-name>
```

Phew. Open the Portal; when deployment completes you will see a _WebJob_ under the app service app with status _Running_.

That means the background worker is writing logs to a Storage Table - find it in the Storage Browser and you should see lots of data going in.

## Switching to Cosmos Table API as the log sink

The app is still running and generating logs. To prove that CosmosDB is a drop-in replacement for tables all we need to do is change the connection string, so the app starts writing to Cosmos instead.

📋 Print the _Primary Table Connection String_ from the list of keys for the Cosmos DB.

<details>
  <summary>Not sure how?</summary>

The key list is at the database account level. It's the same command for all API types; if you print al the keys you'll see SQL and Table connection strings:

```
az cosmosdb keys list --type connection-strings -g labs-cosmos-table  --query "connectionStrings[?description==``Primary Table Connection String``].connectionString" -o tsv -n <cosmos-db-name>
```

</details><br/>

> The table connection string is in the same format as the Storage Account connection string - clients don't need any changes to be able to read the connection data and connect

Change the config setting in the app:

```
az webapp config appsettings set --settings Serilog__WriteTo__0__Args__connectionString='<cosmos-connection-string>' -g labs-cosmos-table -n <web-app-name>
```

Changing configuration causes the webjob to restart - in the Portal if you refresh you may see the status change to _Stopped_ and then _Running_ again.

When the web job starts up, it will be writing data to Cosmos.

## Lab

Query CosmosDB using Data Explorer to find just the error logs in the last hour. How does the query approach compare to the original Table Storage, or to Cosmos with the SQL API?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG to clean up:

```
az group delete -y -n labs-cosmos-table --no-wait
```