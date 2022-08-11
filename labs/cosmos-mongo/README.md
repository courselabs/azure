# Cosmos DB - Mongo API

## Reference

- [`az cosmosdb mongodb` commands](https://docs.microsoft.com/en-us/cli/azure/cosmosdb/mongodb?view=azure-cli-latest)

- [MongoDB C# driver documentation](https://mongodb.github.io/mongo-csharp-driver/2.17/getting_started/)

## Create a Cosmos DB Mongo database

Portal - create "cosmos"; choose the Mongo API:

- mongo API version
- same capacity/provisioning options
- same geo-redundancy, networking & backup policy

Create a Cosmos DB account with the CLI:

```
az group create -n labs-cosmos-mongo -l westeurope --tags courselabs=azure

az cosmosdb create --help

az cosmosdb create -g labs-cosmos-mongo --enable-public-network --kind MongoDB --server-version 4.2 -n labs-cosmos-mongo-es2 # <unique-dns-name>
```

> Takes longer to create than SQL API

Open in portal - different options, no PowerBI integration; adds _Data migration_ and _Connection string_ left nav options

Create a database using the Mongo API:

```
az cosmosdb mongodb database create --help

az cosmosdb mongodb database create --name AssetsDb -g labs-cosmos-mongo --account-name labs-cosmos-mongo-es2
```

## Connect to the database with Mongo Shell

Open _Data explorer_ in the Portal - you'll see the new AssetsDb database listed. Expand it and there are no collections - the database is empty.

There's a new menu option _Open Mongo shell_ which starts a command-line interface to connect to the database. Run these commands to explore the database:

```
show dbs

use AssetsDb

show collections

db.help()
```

Create a collection:

```
db.createCollection('Students')
```

> Response is JSON

Insert some data:

```
db.Students.help()

db.Students.insertOne({ "OrganizationId": "aed1895", "StudentId" : "aed1895", "FullName": "a b c", "CountryCode": 123 })
```

> Response includes generated object ID

You can insert multiples, and documents don't need to have the same schema:

```
db.Students.insertMany([{ "OrganizationId": "org1", "StudentId" : "1023", "FirstName": "x", "LastName": "y", "Role": "z" },  {"OrganizationId": "org1", "StudentId" : "1040", "FirstName": "a", "LastName": "b", "Role": "c" }])
```

Query the data:

```
db.Students.find().pretty()

db.Students.find( {"OrganizationId" : "org1"} )

db.Students.find( {"OrganizationId" : "org1"}, { FirstName:1, LastName:1 } )
```


## Deploy an app using Cosmos DB with Mongo driver

Get the primary connection string:

```
az cosmosdb keys list --type connection-strings -g labs-cosmos-mongo -n labs-cosmos-mongo-es2
```

- [EntityBase.cs](src/asset-manager/Model/Spec/EntityBase.cs) - base class with ID options
- [MongoAssetService.cs](src/asset-manager/Services/MongoAssetService.cs) - Mongo data access
- [Dependencies.cs](src/asset-manager/Dependencies.cs) - dependency injection

Now configure & deploy the app:


**Make sure there is a semi-colon before the AccountKey field in the connection string**

```
cd src/asset-manager

az webapp up --sku S1 -g labs-cosmos-mongo --plan labs-cosmos-mongo-app-plan --os-type Linux  --runtime dotnetcore:6.0 -n labs-cosmos-mongo-app-es # dns name 

az webapp config appsettings set -g labs-cosmos-mongo -n labs-cosmos-mongo-app-es --settings Database__Api='Mongo' ConnectionStrings__AssetsDb='mongodb://labs-cosmos-mongo-es2:lTVKWfGyj3YInQWY79O98tjYmzv9MUiyFv6Th6IMLihYFHcwSpKheZAl1jK64ckz9xgGQdJegh1h4n2SA3htdQ==@labs-cosmos-mongo-es2.mongo.cosmos.azure.com:10255/?ssl=true&replicaSet=globaldb&retrywrites=false&maxIdleTimeMS=120000&appName=@labs-cosmos-mongo-es2@'
```

Browse to the app; it looks the same as the SQL version, except the IDs are Mongo ObjectId format rather than the GUIDs which the SQL API used.


## Explore the data

Check the data in the explorer to verify the new collections are there, and the data is stored:

```
db.getCollectionNames()

db.Locations.find().pretty()
```

> You'll see a strange base64 representation of the GUID id field, which is an empty string for these documents

