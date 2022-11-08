## Functions: CosmosDb to CosmosDb

triggers invoke the function; bindings for input and output

## Reference

- [CosmosDB trigger & binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-cosmosdb-v2?tabs=in-process%2Cfunctionsv2&pivots=programming-language-csharp)


- [Blob storage trigger & binding samples](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/storage/Microsoft.Azure.WebJobs.Extensions.Storage.Blobs#examples)

## Scheduled Function writing to Blob Storage

expolore func cs

- [TimerToBlob/Hearbeat.cs](labs/functions/timer/TimerToBlob/Hearbeat.cs)

For reference:

```
func init CosmosToCosmos --dotnet 

cd CosmosToCosmos

func new --name Translator --template "CosmosDBTrigger"

dotnet add package Microsoft.Azure.WebJobs.Extensions.CosmosDB --version 3.0.10
```

## Test the function locally

Create CosmosDB - NoSQL API, container `test`


create file local.settings.json with:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "HeartbeatOutputStorageConnectionString": "UseDevelopmentStorage=true"
    }
}
```

Start the Azure Storage emulator:

```
docker run -d -p 10000:10000 -p 10001:10001 --name azurite mcr.microsoft.com/azure-storage/azurite
```

Run (maybe change the timer to every 1 minute)

```
func start
```

Should see output

[2022-11-08T05:24:58.366Z] Processing: 1 documents
[2022-11-08T05:24:58.366Z] Translating message for document ID: test02
[2022-11-08T05:24:58.571Z] Added translated document ID: 62e768b9-1eb1-44bf-8985-ab4cb2386295
[2022-11-08T05:24:58.580Z] Executed 'Translator' (Succeeded, Id=9c74627b-3f01-4747-9930-005aa0fa1133, Duration=238ms)

Check docs in Cosmos

> Note that inserting the new item causes the function to run again :)

## Deploy to Azure

Setup:

```
az group create -n labs-functions-timer --tags courselabs=azure -l eastus

az storage account create -g labs-functions-timer --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-timer  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

Pre-reqs:

- a separate storage account for output
- the connection string for the SA set as appsetting `HeartbeatOutputStorageConnectionString`

Deploy (if you set the schedule to every 1 minute, set it back to 5)

```
func azure functionapp publish <function-name>
```

## Lab

How would you design the database to avoid triggering the function twice when a document to translate gets inserted?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Stop the Azure Storage emulator:

```
docker rm -f azurite
```

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-timer
```
