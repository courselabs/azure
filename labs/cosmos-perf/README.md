# Cosmos DB - Performance Provisioning

## Reference

- [SQL queries in Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-query-getting-started)

- [Securing access to data in Cosmos DB](https://docs.microsoft.com/en-us/azure/cosmos-db/secure-access-to-data?tabs=using-primary-key)

## Understanding Request Units (RUs)

400 RU/s - 250K RU/s

Create a Cosmos DB account - no perf settings at account level:

```
az group create -n labs-cosmos-perf  -l westeurope --tags courselabs=azure

az cosmosdb create -g labs-cosmos-perf --enable-public-network --kind GlobalDocumentDB --default-consistency-level Eventual -n labs-cosmos-perf-es # <unique-dns-name>
```

Create a SQL API database with specific performance:

```
az cosmosdb sql database create --name ShopDb -g labs-cosmos-perf --throughput 500 --account-name labs-cosmos-perf-es
```

Create a container for products:

```
az cosmosdb sql container create --help

az cosmosdb sql container create -n Products -g labs-cosmos-perf  -d ShopDb --partition-key-path '/productId' --throughput 400 -a labs-cosmos-perf-es
```

- labs/cosmos-perf/items/products.json - multiple small items

Browse to the container in the Portal and create items by uploading the file `labs/cosmos-perf/items/products.json`. Query the container to select all items - what is the RU cost of the query?

Open container in _Data Explorer_, click _Upload item_ and navigate to file; then click ellipsis for container and _New SQL Query_:

`SELECT * FROM Products` 

> I get 2.4 RUs

Is the RU count the same if you select only product name and price?

SELECT p.name, p.price FROM Products p -> 2.43 RUs

Compare stats - query execution time increases when selecting fields, so small increase in RU

How about querying for one product:

```
SELECT * FROM Products p WHERE p.name = 'p1'

SELECT * FROM Products p WHERE p.productId = '01'
```

> I get 2.82 RUs for both of these, with an index lookup time of 0.05 ms

Create an alternative container for reference data with a different sctructure:

- labs/cosmos-perf/items/refData.json - single document with embedded array
- labs/cosmos-perf/index-policies/refData.json - custom indexing policy, default is all docs

```
az cosmosdb sql container create -n ReferenceData -g labs-cosmos-perf  -d ShopDb --partition-key-path '/refDataType' --idx @labs/cosmos-perf/index-policies/refData.json --ttl 1000 -a labs-cosmos-perf-es
```

> Open in portal and compare index settings for both containers

```
SELECT * FROM ReferenceData r WHERE r.refDataType='Products'

SELECT *
FROM p IN ReferenceData.items
WHERE p.productId='01'
```

- similar RUs; need to experiment with real data

Any query will result in >1 RU because the query itself uses compute power. If you know the object ID  then you can do a [point-read](https://docs.microsoft.com/en-gb/rest/api/cosmos-db/get-a-document), which will be 1RU for documents up to 1KB.

```
curl https://{databaseaccount}.documents.azure.com/dbs/ShopDb/colls/Products/docs/{doc-id}

curl https://labs-cosmos-perf-es.documents.azure.com/dbs/ShopDb/colls/Products/docs/1f0b2601-a9fd-4459-9f67-b866fc140687
```

> Not authorized! Need an auth token

You can generate them with scripts, but the logic is complex - see this [PowerShell example](https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos.Samples/Usage/PowerShellRestApi/PowerShellScripts/ReadItem.ps1)


## Partitioning impact on RUs

> Portal, metrics total RU

## Consistency & replication impact on RUs



##