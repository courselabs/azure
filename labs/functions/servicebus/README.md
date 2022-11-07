## Functions: Blob Storage to SQL Server

triggers invoke the function; bindings for input and output

## Reference

- [Azure Service Bus trigger & binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus?tabs=in-process%2Cextensionv5%2Cextensionv3&pivots=programming-language-csharp)

- [Table storage binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table?tabs=in-process%2Ctable-api%2Cextensionv3&pivots=programming-language-csharp)

- [Table storage Functions .NET SDK](https://github.com/Azure/azure-sdk-for-net/blob/Microsoft.Azure.WebJobs.Extensions.Tables_1.0.0/sdk/tables/Microsoft.Azure.WebJobs.Extensions.Tables/README.md)

- [Service Bus Functions .NET SDK](https://github.com/Azure/azure-functions-servicebus-extension)

## Topic triggered function writing to table storage & queue

expolore func cs

- 

For reference:

```
func init TopicToTableAndQueue --dotnet 

cd TopicToTableAndQueue

dotnet add package Microsoft.Azure.WebJobs.Extensions.ServiceBus --version 5.8.0
dotnet add package Microsoft.Azure.WebJobs.Extensions.Tables --version 1.0.0

func new --name Supplier1Quote --template ServiceBusTopicTrigger
func new --name Supplier2Quote --template ServiceBusTopicTrigger
```

## Test the function locally

Queue & topics must exist

 
Start the Azure Storage emulator:

```
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

create file local.settings.json with SB connection string:


```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "TableInputStorageConnectionString": "UseDevelopmentStorage=true",
        "UploadSqlServerConnectionString": "Data Source=<sql-container-dns-name>;Initial Catalog=func;Integrated Security=False;User Id=sa;Password=AzureD3v!!!;MultipleActiveResultSets=True"
    }
}
```

Run

```
func start
```

In Portal, send a message to the Topic:

```
{
    "QuoteId" : "42bf48b5-8531-48b3-82e0-91af19df6351", 
    "ProductCode": "PR-123",
    "Quantity" : 19
}
```

> Should see output in function:

```
[2022-11-07T21:56:55.436Z] Supplier1 saved quote response for ID: 20bf48b5-8531-48b3-82e0-91af19df6351
[2022-11-07T21:56:55.436Z] Supplier2 saved quote response for ID: 20bf48b5-8531-48b3-82e0-91af19df6351
[2022-11-07T21:56:55.505Z] Executed 'Supplier1Quote' (Succeeded, Id=58895544-27e9-4a52-99a2-7cf0ff3596c9, Duration=117ms)
[2022-11-07T21:56:55.505Z] Executed 'Supplier2Quote' (Succeeded, Id=31a6f011-0cdf-4941-90d8-92e42031c8f7, Duration=117ms)
```

Check in table storage:

```

az storage table list --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"

```

> Should see quotes

Check in SB queue - should see responses from both

## Deploy to Azure

Setup:

```
az group create -n labs-functions-servicebus --tags courselabs=azure -l eastus

az storage account create -g labs-functions-servicebus --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-servicebus  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

Pre-reqs:

- a service bus namespace with topic `QuoteRequestTopic` and two subscriptions `Supplier1Subscription` & `Supplier2Subscription`
- connection string `ServiceBusInputConnectionString`

- a service bus queue with name `QuoteStoredQueue`
- connection string `ServiceBusOutputConnectionString`

- a storage account for output
- the connection string for the SA set as appsetting `OutputTableStorageConnectionString`

```
func azure functionapp publish <function-name>
```

## Lab

You'd want to test this at scale. How could you use a Function to do that?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Stop the Azure Storage emulator:

```
docker rm -f azurite
```

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-servicebus
```
