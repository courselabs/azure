# Functions: Service Bus to Multiple Outputs

Functions and messaging go together very nicely when you build apps using asynchronous event publishing. Your main app might push messages to a Service Bus topic and that can trigger a function which adds a new feature.

In this lab we'll use the Service Bus trigger and see how functions look with multiple outputs - in this case we'll write to Table Storage and publish a message to a Service Bus queue.

## Reference

- [Azure Service Bus trigger & binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus?tabs=in-process%2Cextensionv5%2Cextensionv3&pivots=programming-language-csharp)

- [Table storage binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table?tabs=in-process%2Ctable-api%2Cextensionv3&pivots=programming-language-csharp)

- [Table storage Functions .NET SDK](https://github.com/Azure/azure-sdk-for-net/blob/Microsoft.Azure.WebJobs.Extensions.Tables_1.0.0/sdk/tables/Microsoft.Azure.WebJobs.Extensions.Tables/README.md)

- [Service Bus Functions .NET SDK](https://github.com/Azure/azure-functions-servicebus-extension)

## Service Bus Topic function writing to Table Storage & queue

The scenario is an application where we have multiple suppliers who can quote to provide products. There are two functions in the `TopicToTableAndQueue` directory:

- [TopicToTableAndQueue/Supplier1Quote.cs](/labs/functions/servicebus/TopicToTableAndQueue/Supplier1Quote.cs) - listens on a topic subscription for an incoming request, saves the response to table storage and posts a message to a queue

- [TopicToTableAndQueue/Supplier2Quote.cs](/labs/functions/servicebus/TopicToTableAndQueue/Supplier2Quote.cs) - does the same as `Supplier1Quote` but with a different pricing engine and a deliberate delay

These attibutes take care of the trigger and bindings:

- `[ServiceBusTrigger]` sets the function to run when messages are delivered to the `QuoteRequestTopic` and either the `Supplier1Subscription` or `Supplier2Subscription`
- `[Table]` is an output binding which will create an entity in Table Storage in the  table `quotes`
- `[ServiceBus]` is another output binding which will send a message to the queue `QuoteStoredQueue` to notify that the quote has been saved

Service Bus topics can have multiple subscriptions, which each get a copy of every message. This scenario models two separate processes (which in the real world would be calling different supplier APIs), which have different latencies. Each process is a function with its own subsciption so they can work at their own pace.

<details>
  <summary>For reference</summary>

Here's how the function was created:

```
func init TopicToTableAndQueue --dotnet 

cd TopicToTableAndQueue

dotnet add package Microsoft.Azure.WebJobs.Extensions.ServiceBus --version 5.8.0
dotnet add package Microsoft.Azure.WebJobs.Extensions.Tables --version 1.0.0

func new --name Supplier1Quote --template ServiceBusTopicTrigger
func new --name Supplier2Quote --template ServiceBusTopicTrigger
```

</details><br/>

## Test the function locally

There are no Service Bus emulators, so you can't run the function entirely locally, you'll need to create this dependency in Azure first:

- a Service Bus Namespace (Standard SKU)
- a queue called `QuoteStoredQueue` in the namespace
- a topic called `QuoteRequestTopic` in the namespace 
- two subsctriptions called `Supplier1Subscription` and `Supplier2Subscription` in the topic

Start the Azure Storage emulator which the function will use:

```
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

And create the file `labs/functions/servicebus/TopicToTableAndQueue/local.settings.json` **with your own Service Bus connection string**:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "ServiceBusInputConnectionString" : "<your-connection-string>",
        "ServiceBusOutputConnectionString" : "<your-connection-string>",
        "OutputTableStorageConnectionString": "UseDevelopmentStorage=true"
    }
}
```

You can run the function locally when you have all the dependencies:

```
cd labs/functions/servicebus/TopicToTableAndQueue

func start
```

In the Azure Portal, send a message to the Topic:

```
{
    "QuoteId" : "42bf48b5-8531-48b3-82e0-91af19df6351", 
    "ProductCode": "PR-123",
    "Quantity" : 19
}
```

> You should see output like this in your function:

```
[2022-11-07T21:56:55.436Z] Supplier1 saved quote response for ID: 42bf48b5-8531-48b3-82e0-91af19df6351
[2022-11-07T21:56:55.436Z] Supplier2 saved quote response for ID: 42bf48b5-8531-48b3-82e0-91af19df6351
[2022-11-07T21:56:55.505Z] Executed 'Supplier1Quote' (Succeeded, Id=58895544-27e9-4a52-99a2-7cf0ff3596c9, Duration=117ms)
[2022-11-07T21:56:55.505Z] Executed 'Supplier2Quote' (Succeeded, Id=31a6f011-0cdf-4941-90d8-92e42031c8f7, Duration=117ms)
```

And you can check the `quotes` table has been created in the emulator (or use Storage Explorer to browse):

```

az storage table list --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"
```

And you can check in the Service Bus **queue** by peeking messages from the start of the queue - you should see responses from both suppliers.

You can use your Service Bus namespace for the Azure deployment (or you might prefer to create new ones).

## Deploy to Azure

Here's the setup to get you started:

```
az group create -n labs-functions-servicebus --tags courselabs=azure -l eastus

az storage account create -g labs-functions-servicebus --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-servicebus  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

The pre-reqs for your function are:

- a service bus namespace with topic `QuoteRequestTopic` and two subscriptions `Supplier1Subscription` & `Supplier2Subscription`
- the connection string for the for the Service Bus as appsetting `ServiceBusInputConnectionString`

- a service bus queue with name `QuoteStoredQueue`
- the connection string for the for the Service Bus as appsetting `ServiceBusOutputConnectionString`

- a storage account for output
- the connection string for the for the storage account as appsetting `OutputTableStorageConnectionString`


Then you'll be ready to publish:

```
func azure functionapp publish <function-name>
```

## Lab

You'd want to test this at scale. How could you use a Function to do that?

> Stuck? Try [suggestions](suggestions.md).

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
