# Lab Solution

You'll see the object ID at the bottom of the results when you fetch a document. My reference data document looks like this:

```
                "productId": "999",
                "name": "p999",
                "price": 4995
            }
        ],
        "id": "b9937d9c-fdd9-44c7-8e35-80d1befba841",
        "_rid": "3UsvAOBTm7cBAAAAAAAAAA==",
        "_self": "dbs/3UsvAA==/colls/3UsvAOBTm7c=/docs/3UsvAOBTm7cBAAAAAAAAAA==/",
        "_etag": "\"67019b48-0000-0d00-0000-635ca24e0000\"",
        "_attachments": "attachments/",
        "_ts": 1667015246
    }
]
```

The reference data type is the partition key, so I can try to get a point-read like this:

```
SELECT * 
FROM ReferenceData r 
WHERE r.refDataType="Products" 
AND r.id="b9937d9c-fdd9-44c7-8e35-80d1befba841"
```

That gives me these results:

| Retrieved Document Count | Retrieved Document Size | Execution Time | RUs|
|-|-|-|-|
|59958|60258|0.01ms|3.59|


Which is unexpected - I have partition key and object ID and the returned document is small, so this should be 1RU.

**But** the data explorer can only execute SQL queries, [point-reads have to be done using a client library](https://devblogs.microsoft.com/cosmosdb/point-reads-versus-queries/).

In C# I could write code like this:

```
var refData = await container.ReadItemAsync<RefData>(id: "b9937d9c-fdd9-44c7-8e35-80d1befba841", partitionKey: new PartitionKey("Products"));
```

And that would cost 1RU. Going back to our app example, this would be 2% of the throughput capacity (10 * 1 * 1 = 10 RUs out of 500 RU/s).

> To navigate data effectively we should set a custom value in the _id_ field and not let Cosmos generate a random one for us.

