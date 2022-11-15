# Functions: CosmosDB to CosmosDB

CosmosDB can be used as a trigger and an output for Azure Functions. The trigger is for created and edited documents, but the potential scale of CosmosDB means each call to the function could have multiple documents, so the logic needs to allow for that. In some cases you want to use the same database collection for input and output, but that also needs careful thinking about.

In this lab we'll use CosmosDB as the trigger and the output for a function.

## Reference

- [CosmosDB trigger & binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2?tabs=in-process%2Cfunctionsv2&pivots=programming-language-csharp)


## CosmosDB function writing to CosmosDB

The scenario is a translation engine. When documents are saved to CosmosDB with an English language message, the function translates them to Spanish and stores the translated document in the same CosmosDB database. The code is in the `CosmosToCosmos` directory:

- [CosmosToCosmos/Translator.cs](/labs/functions/cosmos/CosmosToCosmos/Translator.cs) - receives incoming documents and translates specific messages from English to Spanish

These attributes wire up the function:

- `[CosmosDBTrigger]` fires with a read-only set of documents when there are additions or updates to the `posts` collection

- `[CosmosDB]` is an output binding which will write a set of documents to the same `posts` collection

The logic is a little more complex than previous labs, but only because of the multiple inputs. The document collection is iterated, and if there's a match on the message and language then the translator fires and adds a new document to the output, which will be created in CosmosDB.

<details>
  <summary>For reference</summary>

Here's how the function was created:

```
func init CosmosToCosmos --dotnet 

cd CosmosToCosmos

func new --name Translator --template "CosmosDBTrigger"

dotnet add package Microsoft.Azure.WebJobs.Extensions.CosmosDB --version 3.0.10
```

</details><br/>

## Test the function locally

There is a [CosmosDB emulator](https://learn.microsoft.com/en-us/azure/cosmos-db/local-emulator?tabs=ssl-netstd21) you can run locally, but it is fiddly to set up. If you use CosmosDB a lot then it's worth going through the steps, but it's a bit much for this lab.

Instead create a CosmosDB account in Azure:

- use the NoSQL API
- create a database called `Test`
- in the database create a collection called `posts`
- use the partition key `/id` for the collection

Then write the CosmosDB connection string into a new file at `labs/functions/cosmos/CosmosToCosmos/local.settings.json`:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "CosmosDbConnectionString" : "",
        "DatabaseName" : "Test"
    }
}
```

The rest of the components will run locally. Start the Azure Storage emulator:

```
docker run -d -p 10000-10002:10000-10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

Run the function:

```
cd labs/functions/cosmos/CosmosToCosmos

func start
```

Open the CosmosDB Data Explorer in the Portal and insert some new items:

```
{
    "id": "123",
    "message": "goodbye",
    "lang" : "en"
}
```

```
{
    "id": "897",
    "message": "hello",
    "lang" : "en"
}
```

You should see output like this:

```
[2022-11-08T05:24:58.366Z] Processing: 1 documents
[2022-11-08T05:24:58.366Z] Translating message for document ID: test02
[2022-11-08T05:24:58.571Z] Added translated document ID: 62e768b9-1eb1-44bf-8985-ab4cb2386295
[2022-11-08T05:24:58.580Z] Executed 'Translator' (Succeeded, Id=9c74627b-3f01-4747-9930-005aa0fa1133, Duration=238ms)
```

Check the documents in Cosmos, and you should see a translated 'hola' document for each 'hello' document.

> Note that inserting the new item causes the function to run again :)

## Deploy to Azure

For the Azure run you can use your existing CosmosDB database or create a new one. Here's the core setup:

```
az group create -n labs-functions-cosmos --tags courselabs=azure -l eastus

az storage account create -g labs-functions-cosmos --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-cosmos  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

The pre-reqs for the function are:

- a CosmosDB database called `Prod` with a collection called `posts`
- the connection string set in the appsetting `CosmosDbConnectionString`
- the database name set in the appsetting `DatabaseName`

When you're ready you can deploy:

```
func azure functionapp publish <function-name>
```

Test the function with some new documents in the Prod database.

## Lab

How would you design the database to avoid triggering the function twice when a document to translate gets inserted? If your logic was different, could you end up with an infinite loop of triggered functions?

> Stuck? Try [suggestions](suggestions.md).

___

## Cleanup

Stop the Azure Storage emulator:

```
docker rm -f azurite
```

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-cosmos
```
