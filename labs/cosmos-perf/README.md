# Cosmos DB - Performance Provisioning

CosmosDB is charged for storage and compute. Storage is a flat rate, you pay for the amount of data stored and the charge is the same whatever performance level you're using. Compute is charged in terms of _Request Units_ (RUs), you pay for all access operations (read, write, delete, update and query). You can choose between a serverless model where you pay for RUs consumed, or a provisioned model where you pay for a fixed level of RUs.

Cost can be a deterrent for using Cosmos, but if you plan appropriately then it can be a very cost-effective database. In this lab we'll see how you can test and measure RU consumption.

## Reference

- [Serverless mode for Cosmos DB](https://learn.microsoft.com/en-us/azure/cosmos-db/serverless)

- [Consistency levels explained](https://docs.microsoft.com/en-gb/azure/cosmos-db/consistency-levels?toc=%2Fazure%2Fcosmos-db%2Fsql%2Ftoc.json#guarantees-associated-with-consistency-levels)

- [SQL queries in Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-getting-started)

- [Securing access to data in Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/secure-access-to-data?tabs=using-primary-key)

## Create a CosmosDB Container with Fixed Performance

Request Units per second (RU/s) define the level of performance of your CosmosDB. Other factors affect cost too - like the amount of replication and the multiple-writes feature - but RU consumption is the primary cost factor.

Create a new Cosmos DB account:

```
az group create -n labs-cosmos-perf  -l westeurope --tags courselabs=azure

az cosmosdb create -g labs-cosmos-perf --enable-public-network --kind GlobalDocumentDB --default-consistency-level Eventual -n <cosmos-db-name>
```

Create a SQL API database with a specific performance level of 500 RU/s:

```
az cosmosdb sql database create --name ShopDb -g labs-cosmos-perf --throughput 500 --account-name <cosmos-db-name>
```

> This uses the standard provisioning model, where you pay for a level of RU performance and get charged whether you hit that level or not.

The alternative billing models are [serverless](https://learn.microsoft.com/en-us/azure/cosmos-db/scripts/cli/nosql/serverless) and [autoscale](https://learn.microsoft.com/en-us/azure/cosmos-db/scripts/cli/nosql/autoscale).

We'll work directly with the database rather than through an app, so we need to create a document container. Containers can be allocated a portion of the full database throughput, so you can focus performance where you need it.

CosmosDB indexes every field in a document, which speeds up queries at the expense of inserts and storage. We'll use a custom policy which only indexes the ID field:

- [index-policies/products.json](/labs/cosmos-perf/index-policies/products.json) - this is how you express the policy in JSON

📋 Create a SQL container called `Products` for the database. Set the container to have 400 RU/s throughput, use `productId` as the partition key and set the custom index policy from the JSON file.

<details>
  <summary>Not sure how?</summary>

Check the help:

```
az cosmosdb sql container create --help
```

You use `throughput` for fixed performance, or `max-throughput` for autoscale. An ID field is required for documents, which you set in the `partition-key-path`:

```
az cosmosdb sql container create -n Products -g labs-cosmos-perf  -d ShopDb --partition-key-path '/productId' --throughput 400 --idx @labs/cosmos-perf/index-policies/products.json -a <cosmos-db-name>
```

</details><br/>

Now we're all set to add and query some data.

## Estimating RU Usage

How you format your data can make a lot of difference to RU consumption. We have a list of products to save in the shop database, which is reference data we can model in different ways. Here we have one document per product:

- [items/products.json](/labs/cosmos-perf/items/products.json) - this will give us 1000 small documents

📋 Upload the documents in `labs/cosmos-perf/items/products.json` to the container using the Portal. Query the container to select all items - what is the RU cost of the query?

<details>
  <summary>Not sure how?</summary>

Open the container in _Data Explorer_, click _Upload item_ and navigate to the file.

When the data is uploaded, click the ellipsis for the container and select _New SQL Query_. Enter:

```
SELECT * FROM Products
```

When you see the results you can switch to the _Query Stats_ page and see the RU charge.

</details><br/>

> I get 7.46 RUs

Is the RU count the same if you select only product name and price? Open a new SQL Query tab and try it:

```
SELECT p.name, p.price FROM Products p
```

> I get 8.22 RUs

If you compare the stats for both queries, you'll see that query execution time increases when you select fields, so that's more processing time and a small increase in RU. I get this:

|Fields| Retrieved Document Size | Output Document Size | Execution Time | RUs|
|-|-|-|-|-|
|*|59958|60258|0.09ms|7.46|
|p.name, p.price|59958|5568|0.15ms|8.22|

How about querying for one product:

```
SELECT * FROM Products p WHERE p.name = 'p1'

SELECT * FROM Products p WHERE p.productId = '1'
```

> I get 18.59 RUs for the first and 2.82 RUs for the second

The difference here is the index lookup - the name field doesn't have an index, so Cosmos has to read every row:

|Fields| Document Load | Retrieved Document Count | Execution Time | RUs|
|-|-|-|-|-|
|name|1.97ms|1000|0.28ms|18.59|
|id|0.02ms|1|0.01ms|2.82|

These are small numbers, but that's still a big difference. You can see that RUs are calculated from a number of factors. Query execution time and index lookup time are affected if you filter the items or the fields.

With a small dataset it may be cheaper to store it as an array inside a single document. 

## Alternative Data Modelling and RUs

This is a bulk load approach. Application code can fetch all the products in a single document from Cosmos cheaply, and then filter the list in memory. The app can use an expiration cache so the list in memory gets refreshed every few minutes.

We can try this approach by creating an alternative container which we'll use for reference data: 

- [items/refData.json](/labs/cosmos-perf/items/refData.json) - the same product list, represented as an array in a single document

📋 Create a new container called `ReferenceData` using the field `refDataType` as the partition key. We won't supply an index policy for this container.

<details>
  <summary>Not sure how?</summary>

```
az cosmosdb sql container create -n ReferenceData -g labs-cosmos-perf  -d ShopDb --partition-key-path '/refDataType' -a <cosmos-db-name>
```

</details><br/>

> Open in the Portal and compare index settings for both containers

The default is for all fields to be indexed. Indexes use storage too - in some cases where all fields are indexed, the index size might be bigger than the data size.

Upload the documents in `labs/cosmos-perf/items/refData.json` to the new container using the Portal.
Run some queries to fetch all products, and filter for one product:

```
SELECT * FROM ReferenceData r 
WHERE r.refDataType='Products'

SELECT *
FROM p IN ReferenceData.items
WHERE p.productId='1'
```

> I get 3.59 RUs to retrieve all the data, and 4.94 RUs to get the single product

If my app fetched every product individually from Cosmos, then at high scale with 10 instances making 10 queries per second I would nearly hit the throughput limit (10 * 10* 4.94 = 494 out of 500 RU/s). If my app used the first query and instances cached the results for at least one second, then I'd use less than 10% of the throughput (10 * 1 * 3.59 = 35.9 out of 500 RU/s).

You need to model your data and architect your application carefully if you intend to use CosmosDB at high scale.

## Lab

The cheapest way to read individual documents from Cosmos is to fetch them using the object ID and partition key - that's called a _point read_ and costs 1RU (for documents up to 100Kb). 

Find the object ID for the reference data item we inserted in the second approach and check the RU cost to fetch it using the ID and partition key. Do you get 1RU? Are there any expensive parts of the query?


> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG and the database and all the data will be deleted too:

```
az group delete -y --no-wait -n labs-cosmos-perf
```