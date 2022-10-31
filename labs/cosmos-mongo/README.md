# Cosmos DB - Mongo API

The native driver for CosmosDB needs a custom client library in your code, but the other drivers use standard APIs. [MongoDB]() is a popular open-source no-SQL database, and you can create a CosmosDB instance which uses the Mongo driver. That's perfect for moving existing apps to Azure - you don't need to change any code, the app still sees a Mongo database, but you get all the scale and consistent management of Cosmos.

In this lab we'll create a CosmosDB database with the Mongo driver and use it with a simple .NET application.

## Reference

- [CosmosDB for Mongo overview](https://learn.microsoft.com/en-us/azure/cosmos-db/mongodb/introduction)

- [MongoDB C# driver documentation](https://mongodb.github.io/mongo-csharp-driver/2.17/getting_started/)

- [`az cosmosdb mongodb` commands](https://docs.microsoft.com/en-us/cli/azure/cosmosdb/mongodb?view=azure-cli-latest)

## Create a Cosmos DB Mongo database

Create a new Cosmos DB resource in the Portal - choose the Mongo API. Explore the options:

- you can select the Mongo API version
- same capacity/provisioning options as the SQL driver
- same geo-redundancy, networking & backup policy

Under the hood your Cosmos DB storage engine is the same whichever API you choose. The main features of the service are the same for all APIs, the choice is about how you want to applications to connect.

Create a Cosmos DB account with the CLI:

```
az group create -n labs-cosmos-mongo --tags courselabs=azure -l westeurope

az cosmosdb create -g labs-cosmos-mongo --enable-public-network --kind MongoDB --server-version 4.2 -n <cosmos-db-name> 
```

> The `Kind` is set at the account level - either DocumentDB or MongoDG. All the databases in the account need to use the same kind

You might find this takes longer to create than a Cosmos Account for the SQL API. When it's done, open the CosmosDB Account in portal. There are different options from a document DB account:

- there is no PowerBI integration
- there are new _Data migration_, _Connection string_ and _Collection_ left nav options

📋 Create a database in the account called `AssetsDb` using the `cosmosdb mongodb database create` command.

<details>
  <summary>Not sure how?</summary>

```
az cosmosdb mongodb database create --help
```

As a minimum you need to set the name, account name and RG:

```
az cosmosdb mongodb database create --name AssetsDb -g labs-cosmos-mongo --account-name <cosmos-db-name>
```

</details><br/>

## Connect to the database with Mongo Shell

Open _Data explorer_ in the Portal - you'll see the new AssetsDb database listed. Expand it and there are no collections, it's a new empty database.

There's a new menu option _Open Mongo shell_ which starts a command-line interface to connect to the database. Select that option and in the terminal run these commands to explore the database:

```
show dbs

use AssetsDb

show collections

db.help()
```

The NoSQL API uses a customized version of SQL to work with data in the Portal, but the Mongo commands are standard for any Mongo database.

Data in Mongo is stored as documents in collections (broadly similar to rows in tables for a SQL database). Create a collection:

```
db.createCollection('Students')
```

> The response is JSON - the native data format for Mongo

Insert some data:

```
db.Students.help()

db.Students.insertOne({ "OrganizationId": "aed1895", "StudentId" : "aed1895", "FullName": "a b c", "CountryCode": 123 })
```

> The JSON response includes the object ID which the database generated

You can insert multiple documents with one command, and documents don't need to have the same schema:

```
db.Students.insertMany([{ "OrganizationId": "org1", "StudentId" : "1023", "FirstName": "x", "LastName": "y", "Role": "z" },  {"OrganizationId": "org1", "StudentId" : "1040", "FirstName": "a", "LastName": "b", "Role": "c" }])
```

📋 Query the data using the `find` method on the collection. Can you find the students in organization `org1`, but only include the first and last name in the results?

<details>
  <summary>Not sure how?</summary>

Print the help text:

```
db.Students.find().help()
```

It's all there, but it's not as helpful as the Azure CLI help text.

Show all the documents:

```
db.Students.find().pretty()
```

Query by a property - your query is expressed as a JSON object:

```
db.Students.find( {"OrganizationId" : "org1"} )
```

And project properties in the response - use 1 to include the field and 0 to exclude it:

```
db.Students.find( {"OrganizationId" : "org1"}, { _id:0, FirstName:1, LastName:1 } )
```

</details><br/>

The MongoDB syntax takes some getting used to - but it is very consistent. The same functions and formats you can use in the shell are there in client libraries for your code too.

## Run an app using Cosmos DB with Mongo

Documents in Mongo map directly to objects in code, so you don't need an ORM layer to convert your app's representation to the database representation. The Mongo client libraries effectively fetch JSON from the database and [deserialize]() it into objects.

Your object classes do need to include some information for Mongo:

- [EntityBase.cs](/src/asset-manager/Model/Spec/EntityBase.cs) - this is the object base class, it uses annotations from the Mongo client library, to specify which property is the object ID
- [MongoAssetService.cs](/src/asset-manager/Services/MongoAssetService.cs) - manages data access, loading documents from collections and inserting the reference data
- [Dependencies.cs](/src/asset-manager/Dependencies.cs) - manages the different storage options for the app can use, Mongo is supported using the standard client library, nothing CosmosDB-specific 

📋 Print the _Primary MongoDB Connection String_ from the list of keys for the Cosmos DB.

<details>
  <summary>Not sure how?</summary>

The key list is at the database account level. It's the same command for all API types, but the names of the keys is different from the SQL API:

```
az cosmosdb keys list --type connection-strings -g labs-cosmos-mongo  --query "connectionStrings[?description==``Primary MongoDB Connection String``].connectionString" -o tsv -n <cosmos-db-name>
```

</details><br/>

> The connection string starts with `mongodb://<cosmos-db-name>` - it contains authentication details so it needs to be treated as secure data

Run the app locally (you'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download), using parameters to set the database type and connection string:

```
# be sure to quote the connection string:
dotnet run --project src/asset-manager --Database:Api=Mongo --ConnectionStrings:AssetsDb='<cosmos-connection-string>'
```

Browse to the app at http://localhost:5208. When it runs it will create some collections in Mongo and save some documents. It's the same application we used in the [Cosmos lab](), but with Mongo the object IDs are in a different format (is there a pattern to the IDs?).



## Lab

Use the Mongo shell (or the [Mongo VS Code]() extension) to find all the location documents. How is the data structure different from Cosmos SQL API? Try to insert a new location:

```
{
    "AddressLine1": "1 Parliament Place",
    "Country": "Singapore",
    "PostalCode": "178880"
}
```

Can you save it in the database, so it appears in the app when you refresh?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG and the database and all the data will be deleted too:

```
az group delete -y --no-wait -n labs-cosmos-mongo
```
