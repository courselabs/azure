## Functions: RabbitMQ to Blob Storage

triggers invoke the function; bindings for input and output

## Reference

- [Azure Service Bus trigger & binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus?tabs=in-process%2Cextensionv5%2Cextensionv3&pivots=programming-language-csharp)

- [Table storage binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-table?tabs=in-process%2Ctable-api%2Cextensionv3&pivots=programming-language-csharp)

- [Table storage Functions .NET SDK](https://github.com/Azure/azure-sdk-for-net/blob/Microsoft.Azure.WebJobs.Extensions.Tables_1.0.0/sdk/tables/Microsoft.Azure.WebJobs.Extensions.Tables/README.md)

- [Service Bus Functions .NET SDK](https://github.com/Azure/azure-functions-servicebus-extension)

- [Azurite connection strings](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json&tabs=visual-studio#http-connection-strings)

## Topic triggered function writing to table storage & queue

expolore func cs

- 

For reference:

```
func init RabbitToBlob --dotnet 

cd RabbitToBlob

dotnet add package Microsoft.Azure.WebJobs.Extensions.RabbitMQ --version 2.0.3

# no template, manually write CS
```

## Test the function locally
 
Start the Azure Storage emulator & RabbitMQ:

```
docker compose -f labs/functions/rabbitmq/docker-compose.yml up -d
```

Open the RabbitMQ UI:

http://localhost:15672

Sign in with guest/guest and create a queue called `customerevents`

create file local.settings.json with connection details:


```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "TableInputStorageConnectionString": "UseDevelopmentStorage=true",
        "xxx
    }
}
```

Run

```
func start
```

Open the queue in Rabbit UI and publish some messages:

```
{
  "CustomerId" : 113435,
  "EventType" : "Complaint"
}
```

> Should see output in function:

```
[2022-11-08T02:58:32.148Z] Received customer message with event type: Complaint
[2022-11-08T02:58:32.166Z] Archiving complaint message for customer: 113435
[2022-11-08T02:58:32.291Z] Executed 'PriorityMessageArchive' (Succeeded, Id=44d45f7c-91b4-445d-9c6c-ea604fa0c614, Duration=145ms)
```

Check in table storage:

```
az storage blob list -c complaints -o table --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
```

> Should see complaints


## Deploy to Azure

Setup:

```
az group create -n labs-functions-servicebus --tags courselabs=azure -l eastus

az storage account create -g labs-functions-servicebus --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-servicebus  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

Pre-reqs:

- rabbitmq - no managed az service, search for 'rabbitmq bitnami' and create one of the options (VM is the easiest option, you can use a dev/test preset and then you're in the normal VM create screen; use password instead of SSH key, 

- you'll need to set the NSG to allow access to port 15672 - best if that and 22 are restricted to your own IP address; 5672 from any address

- follow [these instructions to get the RabbitMQ username & password](https://docs.bitnami.com/azure/faq/get-started/find-credentials/#option-2-find-credentials-by-connecting-to-your-application-through-ssh))

- connect & follow same steps to create queue `customerevents`

- connection string for rabbit in format `amqp://user:password@ip-address:5672`

- a storage account for output
- the connection string for the SA set as appsetting `OutputTableStorageConnectionString`

```
func azure functionapp publish <function-name>
```

Check rabbit ui - _Connections_ - you should see a connection which is the listener for the Fn trigger.

Publish some messages...

## Lab

How could you automate the RabbitMQ VM setup?

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
