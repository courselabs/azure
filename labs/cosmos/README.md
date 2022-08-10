# Cosmos DB

Planet scale database - one product, different drivers: SQL, Mongo, table storage and more. API is fixed per db but gives consistent mgmt across all system databases.

## Reference

- [Cosmos DB overview](https://docs.microsoft.com/en-gb/azure/cosmos-db/introduction)

- [Consistency levels explained](https://docs.microsoft.com/en-gb/azure/cosmos-db/consistency-levels?toc=%2Fazure%2Fcosmos-db%2Fsql%2Ftoc.json#guarantees-associated-with-consistency-levels)

- [Microsoft Learn: Explore Cosmos DB](https://docs.microsoft.com/en-us/learn/modules/explore-azure-cosmos-db/)

- [`az cosmosdb` commands](https://docs.microsoft.com/en-us/cli/azure/cosmosdb?view=azure-cli-latest)

- [`az cosmosdb sql` commands](https://docs.microsoft.com/en-us/cli/azure/cosmosdb/sql?view=azure-cli-latest)

## Create a Cosmos DB database

Portal - create "cosmos"; choose the SQL API:

- capacity mode - provisioned or serverless
- provisioned allows free tier & price cap
- geo-redundancy, data sync across regions
- backup policy

> Enterprise-grade database, beware of cost

Create a Cosmos DB account with the CLI:

```
az group create -n labs-cosmos  -l westeurope --tags courselabs=azure

az cosmosdb create --help

az cosmosdb create -g labs-cosmos --enable-public-network --kind GlobalDocumentDB -n labs-cosmos-es # <unique-dns-name>
```

Open in portal - this is just an account, a grouping mechanism for databases; quick start guidance

Create a database using the SQL API:

```
az cosmosdb sql database create --help

az cosmosdb sql database create --name AssetsDb -g labs-cosmos --account-name labs-cosmos-es
```

> Check in the Portal - databases under a Comos DB account don't show as separate resources (like apps in an app service plan)

Open the _Data Explorer_ for the account and you can see the new database; it's empty now.

Open _Keys_ to see the connection string for client apps - make a note of the primary; or use:

```
az cosmosdb keys list --type connection-strings -g labs-cosmos -n labs-cosmos-es
```

## Deploy an ORM app using Cosmos DB

- [Asset.cs](src/asset-manager/Entities/Model/Asset.cs) - POCO model with relationships
- [AssetContext.cs](src/asset-manager/Entities/AssetContext.cs) - EF context
- [Program.cs](src/asset-manager/Program.cs) - app configuration using Cosmos


Now configure & deploy the app:

**Make sure there is a semi-colon before the AccountKey field in the connection string**

```
cd src/asset-manager

az webapp up --sku S1 -g labs-cosmos --plan labs-cosmos-app-plan --os-type Linux  --runtime dotnetcore:6.0 -n labs-cosmos-app-es # dns name 

az webapp config appsettings set -g labs-cosmos -n labs-cosmos-app-es --settings ConnectionStrings__AssetsDb='AccountEndpoint=https://labs-cosmos-es.documents.azure.com:443/;AccountKey=57RTIHQVOn01wxSsiXGlay3EBoSQ5Sufwe3iLYIgdDZ3BCdbgwv9PjLkCNeCNqRoI8O905DTEpXRt1I9osVlLA==;'


```

Browse to the app. 

## Explore the data

The web app seeds reference data when it starts. In the Cosmos Data browser you can see the _containers_ - one container is used for all the object types in this app.

Check the items and you'll see locations and asset types.

Add a new location (e.g. copy an existing one - not the `_` fields):

```
{
    "Id": "64eb3e9f-e92d-4a63-b234-08da7b01d0d6",
    "AddressLine1": "2600 Pennsylvania Ave NW",
    "Country": "USA",
    "Discriminator": "Location",
    "PostalCode": "DC 20500",
    "id": "Location|64eb3e9f-e92d-4a63-b234-08da7b01d0d6"
}
```

- Discriminator is an EF mechanism for identifying object type
- Id is an object property, id is the item identifier (includes Discriminator)