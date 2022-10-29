# Lab Hints

Fetching a document directly from its object ID and partition key is called a [point-read](https://docs.microsoft.com/en-gb/rest/api/cosmos-db/get-a-document). 

Using any of the internal document fields requires an index lookup. The internal object ID used by Cosmos doesn't need any lookups, and the partition key means there's no searching between partitions, so the query can just read and return the document.

But you can't execute a point-read with every type of client...

> Need more? Here's the [solution](solution.md).
