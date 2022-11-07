## Functions: Timer to Blob Storage

triggers invoke the function; bindings for input and output

## Reference

- [Coding and testing Functions locally](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-local)

- [Timer trigger & binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=in-process&pivots=programming-language-csharp)

- [Functions binding expressions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-expressions-patterns)

- [Blob storage trigger & binding samples](https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/storage/Microsoft.Azure.WebJobs.Extensions.Storage.Blobs#examples)

## Scheduled Function writing to Blob Storage

expolore func cs

- [TimerToBlob/Hearbeat.cs](labs/functions/timer/TimerToBlob/Hearbeat.cs)

For reference:

```
func init TimerToBlob --dotnet 

cd TimerToBlob

func templates list

func new --name Heartbeat --template "Timer trigger"

dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage.Blobs --version 5.0.0
```

## Test the function locally


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

Check files written in storage:

```
docker logs azurite
```


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

How would you add a diagnostic check for another component? More code in this function or a separate function? If you had an API which reported the current status for all components, how would it read the data?

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
