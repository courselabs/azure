# Lab Solution

Open _Data Explorer_ and navigate to the _FulfilmentLogs_ entites. 

In the _Query Builder_ you can set the properties to filter on:

- remove the default `PartitionKey` and `RowKey` fields
- add a new clause where the field `Level` is equal to `Error`
- add a new clause where the field `Timestamp` is greater than or equal to `Last Hour`

> The Explorer knows this is a date/time field and gives you a restricted set of ranges to choose from

Check the _Query Text_ and you'll see this looks very much like the OData filters we used with Table Storage.

CosmosDB also supports querying over HTTP with OData - but you can't generate a SAS token to access Cosmos. Instead you need to [build an authentication header](https://learn.microsoft.com/en-us/rest/api/storageservices/authorize-with-shared-key), which is not straightforward.