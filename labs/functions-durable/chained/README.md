# Durable Functions: Chained Functions

You can chain functions together if the output of one function can serve as the trigger to the next - you might write to blob storage in one function and use that in the blob trigger for the next. That lets you model a workflow with multiple steps, but you can't necessarily guarantee the running order and you won't always have an output to use for the next trigger.

Azure has _durable functions_ for long-running scenarios like this, where state needs to be shared between steps in the workflow. They're deployed as standard Azure Functions, but the trigger starts an _orchestrator_ where the actual code is. The orchestrator code calls all the other activities, managing inputs and outputs without any more triggers.

In this lab we'll use a durable function for a workflow with several activities which need to run in sequence. 

## Reference

- [Durable functions overview](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp)

- [Orchestrations - the coded workflow for a durable function](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-orchestrations?tabs=csharp)

- [Activity functions - the individual steps of the workflow](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-types-features-overview#activity-functions)

## Timer Trigger with Orchestration

The scenario is an alternative implementation of the chained function in the [functions CI/CD lab](/labs/functions/cicd/README.md). The logic is the same, but instead of multiple functions triggering each other we just have one durable function. The code is in the `DurableChained` folder:

- [DurableChained/TimedOrchestratorStart.cs)](/labs/functions-durable/chained/DurableChained/TimedOrchestratorStart.cs) - uses a timer trigger and has a `DurableClient` decorator; it uses that object to start the orchestrator, passing it a dummy application status object

- [DurableChained/ChainedOrchestrator.cs](/labs/functions-durable/chained/DurableChained/ChainedOrchestrator.cs) - this is the orchestrator for all the other activities; it runs three in sequence, using the output from the first as input to the other two

You can see how data is exchanged in a durable function - the trigger can pass an object to the orchestrator, and the orchestrator can pass and receive objects from the actitivities.

These are the activities called by the orchestrator, they use an `ActivityTrigger` so they can only be activated within a durable function:

- [Activities/WriteBlob.cs](/labs/functions-durable/chained/DurableChained/Activities/WriteBlob.cs) - stores the activity status object in a blob; creates the binding in code so we can specify the blob name dynamically

- [Activities/NotifySubscribers.cs](/labs/functions-durable/chained/DurableChained/Activities/NotifySubscribers.cs) - uses a Service Bus binding to write a message to a queue
 
- [Activities/WriteLog.cs](/labs/functions-durable/chained/DurableChained/Activities/WriteLog.cs) - uses a Table binding to write an entity to Table Storage

The activity logic is pretty much the same as an ordinary function, but they are controlled by the orchestrator.

## Test the function locally

There's no Service Bus emulator, so you'll need to create in Azure:

- a Service Bus Namespace
- with a Queue called `HeartbeatCreated`
- and make a note of the connection string

Run Docker Desktop and start the Azure Storage emulator:

```
docker run -d -p 10000-10002:10000-10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

Now create the local configuration file at `labs/functions-durable/chained/DurableChained/local.settings.json` and add your connection settings:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "StorageConnectionString": "UseDevelopmentStorage=true",
        "ServiceBusConnectionString" : "<sb-connection-string>"
    }
}
```

Run the function locally:

```
func start
```

This uses the timed trigger, so within a few minutes you should see the orchestrator being started.

> The function should produce a lot of output, in it you'll see lines like this:

```
[2022-11-14T02:24:00.053Z] Executing 'TimedOrchestratorStart' (Reason='Timer fired at 2022-11-14T02:24:00.0336490+00:00', Id=56d226b5-ba43-46dc-8adf-713b23dd7b45)
[2022-11-14T02:24:00.061Z] Starting orchestration for: save-handler; at: 14/11/2022 02:24:00 (UTC)
...
[2022-11-14T02:24:00.246Z] Executing 'WriteBlob' (Reason='(null)', Id=557d0579-ea38-41c6-8fd5-f3bb8d4ece42)
[2022-11-14T02:24:00.356Z] Created blob: heartbeat/20221114022400
...
[2022-11-14T02:24:00.405Z] Executing 'NotifySubscribers' (Reason='(null)', Id=664c8c69-3f8b-4487-9f5d-7daa3d89865c)
[2022-11-14T02:24:00.845Z] Published heartbeat message
...
[2022-11-14T02:24:00.972Z] Orchestrator completed.
[2022-11-14T02:24:00.973Z] Executed 'ChainedOrchestrator' (Succeeded, Id=54945bb0-80a0-4dbb-9a30-3025404142b2, Duration=1ms)
```

Check in table storage - you should see a table named `heartbeats` (or use Storage Explorer):

```

az storage table list --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;"

```

Open the Service Bus Queue in the Portal and navigate to _Service Bus Explorer_. In the Explorer click _Peek from start_ and you should see messages with body content like this:

```
{
    "BlobName": "heartbeat/20221114022400"
}
```

When it's looking good, you can deploy to Azure.

## Deploy to Azure

Here's the basic setup for your Function App:

```
az group create -n labs-functions-durable-chained --tags courselabs=azure -l eastus

az storage account create -g labs-functions-durable-chained --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-durable-chained  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

For the dependencies you will need:

- a Service Bus Namespace (you can use your existing one) 
- a Service Bus Queue with the name `HeartbeatCreated`
- the connection string for the Service Bus stored in the app setting  `ServiceBusConnectionString`

- a storage account for output
- the connection string for the Storage Account set in appsetting `StorageConnectionString`

```
func azure functionapp publish <function-name>
```

Check the _Functions_ list in the Portal - do all the functions get shown, or just the ones with external triggers (like the timer trigger)?

## Lab

Functions can be set to _Disabled_ in the Portal, which means the trigger won't fire. You could stop this whole workflow by disabling the timer trigger, but can you disable one of the activity triggers? What would happen if you could?

> Stuck? Try [suggestions](suggestions.md) 
___

## Cleanup

Stop the Azure Storage emulator:

```
docker rm -f azurite
```

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-durable-chained
```
