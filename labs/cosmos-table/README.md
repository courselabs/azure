# Cosmos DB - Table API

Azure Table Storage compatbile API - straight replacment for table storage but with Cosmos scale & management

## Reference

- [`az cosmosdb table` commands](https://docs.microsoft.com/en-us/cli/azure/cosmosdb/table?view=azure-cli-latest)


## Create a Cosmos DB Table database

Portal - create "cosmos"; choose the Table API:

- same options as SQL API

Create a Cosmos DB account with the CLI:

```
az group create -n labs-cosmos-table -l westeurope --tags courselabs=azure

az cosmosdb create --help

az cosmosdb create -g labs-cosmos-table --enable-public-network --kind GlobalDocumentDB --capabilities EnableTable -n labs-cosmos-table-es2 # <unique-dns-name>
```

- Open in Portal - different options and integrations again; azure functions; no PowerBI


Create a database using the Table API:

```
az cosmosdb table create --help

az cosmosdb table create --name FulfilmentLogs -g labs-cosmos-table --account-name labs-cosmos-table-es2
```

- open _Data Explorer_, see FulfilmentLogs table, can browse entities but nothing there


## Using table storage as a log sink

Same app we used in Table Storage lab

- [appsettings.json](src/fulfilment-processor/appsettings.json) - configuration to use Table Storage with Serilog;

Print the connection strings for the account:

```
az cosmosdb keys list --type connection-strings -g labs-cosmos-table -n labs-cosmos-table-es2
```

> There are separate SQL and Table connection strings

The table connection string is in a similar format as a storage account connection string:

- `DefaultEndpointsProtocol=https;AccountName=labs-cosmos-table-es2;AccountKey=xxrdb48Zm9y3UmrFjHQaBvQQ8KJZprSUOvB7AoqnSMShHoc6ChsGIZBC3uEmzLSF2EZ6NdLhW3rorjIEJENjOg==;TableEndpoint=https://labs-cosmos-table-es2.table.cosmos.azure.com:443/;`

> You can also see the full connection string in the Portal, under _Connection String_

We'll deploy the app WebJob and update the connection string:

```
az appservice plan create -g labs-cosmos-table  -n app-plan-01 --sku B1 --number-of-workers 1

az webapp create -g labs-cosmos-table --plan app-plan-01 --runtime dotnet:6 -n labs-cosmos-table-es # <dns-unique-name>

az webapp config set --always-on true   -g labs-cosmos-table -n labs-cosmos-table-es # <dns-unique-name>
```

Now configure & deploy the app:

```
az webapp config appsettings set --settings Serilog__WriteTo__0__Args__connectionString='<connection-string>' -g labs-cosmos-table -n labs-cosmos-table-es # <dns-unique-name> 

az webapp config appsettings set --settings Serilog__WriteTo__0__Args__connectionString='DefaultEndpointsProtocol=https;AccountName=labs-cosmos-table-es2;AccountKey=xxrdb48Zm9y3UmrFjHQaBvQQ8KJZprSUOvB7AoqnSMShHoc6ChsGIZBC3uEmzLSF2EZ6NdLhW3rorjIEJENjOg==;TableEndpoint=https://labs-cosmos-table-es2.table.cosmos.azure.com:443/;' -g labs-cosmos-table -n labs-cosmos-table-es # <dns-unique-name>

az webapp deployment source config-zip -g labs-cosmos-table  --src src/fulfilment-processor/deploy.zip -n labs-cosmos-table-es
```

Open the portal; when deployment completes you will see a _WebJob_ under the app service app with status _Running_.

## Explore the data

Now check the new FulfilmentLogs container in the Cosmos data explorer - you'll see lots of entries

> Same partition key and rowkey - but these are fields for documents in Cosmos

It's much easier to explore the data here - find error logs:

- in query editor remove the partition and row key clauses
- add a clause for _Level = Error_
- run the query