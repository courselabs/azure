# Cosmos DB

Cosmos DB is billed as a _planet scale database_. You can start small with a localized instance, but expand it for global replication and the capacity to handle pretty much any load you can throw at it. Cosmos DB is a single product, but databases support different storage drivers: NoSQL, Mongo, table storage and more. Each database is fixed to use a single driver, but you can use different storage approaches for different applications and have consistent management for them all.

In this lab we'll create a CosmosDB account and use a database with the NoSQL driver.

---
**NoSQL is the native driver for CosmosDB**

_But it was previously called the SQL driver_ 😃

You'll see it referred to as "NoSQL" in the Portal, but the CLI and documentation still call it the "SQL" driver.

---

## Reference

- [Cosmos DB overview](https://docs.microsoft.com/en-gb/azure/cosmos-db/introduction)

- [Microsoft Learn: Explore Cosmos DB](https://docs.microsoft.com/en-us/learn/modules/explore-azure-cosmos-db/)

- [`az cosmosdb` commands](https://docs.microsoft.com/en-us/cli/azure/cosmosdb?view=azure-cli-latest)

- [`az cosmosdb sql` commands](https://docs.microsoft.com/en-us/cli/azure/cosmosdb/sql?view=azure-cli-latest)

## Create a Cosmos DB database

Open the Portal and navigate to the new resource page. Search for "cosmos", select _Azure Cosmos DB_. You can select the API - choose NoSQL:

- capacity mode - provisioned or serverless
- provisioned allows free tier & price cap
- geo-redundancy, data sync across regions & optional multi-region writes
- backup policy for automated data backups & storage

> CosmosDB is an enterprise-grade database; make sure you understand the pricing model

Create a Cosmos DB account with the CLI:

```
az group create -n labs-cosmos --tags courselabs=azure -l westeurope

az cosmosdb create --help

az cosmosdb create -g labs-cosmos --enable-public-network --kind GlobalDocumentDB -n <cosmos-db-name>
```

Open the new resource in the Portal - this is just a CosmosDB _account_, a grouping mechanism for databases. On the resource page there's a _Quick start_ wizard.

Create a database using the SQL API:

```
az cosmosdb sql database create --help

az cosmosdb sql database create --name AssetsDb -g labs-cosmos --account-name <cosmos-db-name>
```

> Check in the Portal - databases under a Comos DB account don't show as separate resources (like apps in an app service plan)

Open the _Data Explorer_ for the account and you can see the new database; it's empty right now, but ready to use. Open _Keys_ to see the connection string for client apps - make a note of the primary connection string.

You can also get the connection string from the CLI:

```
az cosmosdb keys list --type connection-strings -g labs-cosmos -n <cosmos-db-name>
```

📋 Add a query to the CLI command and change the output format, so all you see is the value for the _Primary SQL Connection String_.

<details>
  <summary>Not sure how?</summary>

The query for this needs to select the connectStrings field which contains an array, then search the array for the object where the description field matches the input. Then you can select the connectionString field from the object, and use TSV format to print it without any JSON markers:

```
az cosmosdb keys list --type connection-strings -g labs-cosmos  --query "connectionStrings[?description==``Primary SQL Connection String``].connectionString" -o tsv -n <cosmos-db-name>
```

</details><br/>

> This is the sort of task you need to do when you're scripting, so you can get the values you need to inject into app configuration

## Run an app using Cosmos DB with Entity Framework

CosmosDB scales so well because of the way it partitions data, spreading it around multiple storage locations. Those locations can all be read from and written to at the same time, and CosmosDB can increase capacity just by adding more partitions. 

That process is all managed for you, but it does mean that data in the CosmosDB NoSQL driver has a different format from a standard SQL database.

We have a simple .NET application which can use CosmosDB for storage. It is built with the CosmosDB NoSQL library, but the data model is not Cosmos-specific:

- [Asset.cs](/src/asset-manager/Model/Asset.cs) - this is a POCO (Plain-Old C# Object) which has data fields and relationships
- [AssetContext.cs](/src/asset-manager/Sql/AssetContext.cs) - the EF context object, which provides access to the entity objects
- [Dependencies.cs](/src/asset-manager/Dependencies.cs) - manages the different storage options the app can use, in Sql mode it uses CosmosDB

Run the app locally (you'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download), using parameters to set the database type and connection string:

```
# be sure to quote the connection string:
dotnet run --project src/asset-manager --Database:Api=Sql --ConnectionStrings:AssetsDb='<cosmos-connection-string>'
```

Browse to the app at http://localhost:5208 - you'll see a set of reference data items with random IDs:

![Asset Manager with CosmosDB](/img/asset-manager-cosmos.png)

> The application uses an ORM to set up the database schema, and then inserts this reference data.

Check the logs in your terminal and you'll see lots of SQL statements. These are the queries the ORM generates. 

## Explore the data

In the Cosmos Data browser you can see _containers_ - containers are kind of like tables, except that items in a container don't need to have the same schema.

In this app one container is used for all the object types. Check the items and you'll see _Location_ objects and _AssetType_ objects.

Add a new location using Data Explorer:

```
{
    "Id": "64eb3e9f-e92d-4a63-b234-08da7b01d0d6",
    "AddressLine1": "Parliament House",
    "Country": "Australia",
    "Discriminator": "Location",
    "PostalCode": "2600",
    "id": "Location|64eb3e9f-e92d-4a63-b234-08da7b01d0d6"
}
```

- `Discriminator` is an ORM mechanism for identifying the object type
- `Id` is an object property, `id` is the item identifier (which includes the discriminator)

Refresh the browser with your Asset Manager website. It might take a while to reload - is the new location shown?

What happens if you insert a new item without any ID columns?

```
{
    "AddressLine1": "1 Parliament Place",
    "Country": "Singapore",
    "Discriminator": "Location",
    "PostalCode": "178880"
}
```

📋 Reload the website and you'll see an error. Fix the data so the website loads again and new location shows.

<details>
  <summary>Not sure how?</summary>

CosmosDB will automatically generate the `id` column if you don't specify it for a new item, but it doesn't know the conventions the app is expecting. 

The app wants the unique identifier in the `Id` field, and the `id` field needs to be prefixed with the object type.

Cosmos is happy for you to change properties - select the item in the Data Explorer:

- copy the `id` field to a new field called `Id`
- edit the `id` field, inserting `Location|` in front of the actual identifier

Save your changes, refresh the website and you should see all four locations displayed.

</details>

When you're finished testing the app, run `Ctrl-c` or `Cmd-c` to exit.

## Lab

Is it SQL or NoSQL? The NoSQL driver actually supports SQL queries (but not the full SQL syntax). Run some queries in the Data Explorer to find:

- all the asset types, showing just the ID and description
- the count of locations with a '1' in the postal code

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG and the database and all the data will be deleted too:

```
az group delete -y --no-wait -n labs-cosmos
```