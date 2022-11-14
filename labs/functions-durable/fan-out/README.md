# Durable Functions: Fan-Out

Durable functions have their state persisted in Azure. The orchestrator can wait for activities to complete, and has the logic to retry failed activities - which is perfect for longer transactions which involve multiple third-party systems.

When you have multiple system calls to make and you need them all to complete so you can work on the collected set of results, you can use the _fan-out / fan-in_ pattern. The orchestrator works by starting all the activity functions in parallel and waiting for them all to finish.

In this lab we'll use a durable function which uses an HTTP trigger, and see the additional functionality that provides to check on the status of the function.

## Reference

- [Durable Functions for Fan-Out](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp#human)

- [HTTP features for durable functions](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-http-features?tabs=csharp)

- [Error handling and retries in activity functions](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-error-handling?tabs=csharp)

## HTTP Trigger with Orchestration

The scenario is an improved implementation of the quote engine in the [Service Bus functions lab](/labs/functions/servicebus/README.md). The original function just called some suppliers to quote for an order and stored their responses. This version waits for all the responses and selects the cheapest quote.

The code is in the `QuoteEngine` folder:

- [QuoteEngine/HttpOrchestratorStart.cs](/labs/functions-durable/fan-out/QuoteEngine/HttpOrchestratorStart.cs) - uses an HTTP trigger and returns a set of URLs the client can use to work with the running function

- [DurableChained/ChainedOrchestrator.cs](/labs/functions-durable/fan-out/QuoteEngine/QuoteOrchestrator.cs) - the orchestrator calls the three supplier quote activities _asynchronously_ and waits for them all to complete.

This is a very efficient way to manage multiple service calls - the total duration will be the duration of the longest call, whereas with synchronous calls it would be the total of all durations.

Each activity returns a quote response object and the orchestrator selects the one with the best price. The supplier quote activities are all much the same:

- [Activities/Supplier1Quote.cs](/labs/functions-durable/fan-out/QuoteEngine/Activities/Supplier1Quote.cs) - generates a random quote price and returns it

One of the activities has a delay in it, so we can see that the orchestrator will keep waiting until the slowest service returns.

## Test the function locally

There are no dependencies for this function, other than the standard Storage Account.

Run Docker Desktop and start the Azure Storage emulator:

```
docker run -d -p 10000-10002:10000-10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

You still need the local configuration file, so create a text file at `labs/functions-durable/fan-out/QuoteEngine/local.settings.json` and add the standard settings:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet"
    }
}
```

Run the function locally:

```
cd labs/functions-durable/fan-out/QuoteEngine

func start
```

This uses the HTTP trigger. You will see all the functions listed, and a URL you can call to start the orchestrator.

Open another terminal and call the HTTP trigger function:

```
curl http://localhost:7071/api/HttpOrchestratorStart
```

> You'll see the orchestrator logs in the function terminal, starting like this:

```
[2022-11-14T03:56:07.064Z] Executing 'Supplier1Quote' (Reason='(null)', Id=bf710bb4-164a-472b-acac-04d320913b7d)
[2022-11-14T03:56:07.064Z] SUPPLIER-1 calculating price for quote ID: 5a9b8cb3-055b-461b-8846-12f6d8f930e2
[2022-11-14T03:56:07.064Z] SUPPLIER-1 calculated quote: 480; for ID: 5a9b8cb3-055b-461b-8846-12f6d8f930e2
[2022-11-14T03:56:07.064Z] Executed 'Supplier1Quote' (Succeeded, Id=bf710bb4-164a-472b-acac-04d320913b7d, Duration=0ms)
```

And in your curl window you should see a JSON response full of URLs which looks like this:

```
{
    "id":"13ce7b3e0da8405cb12781acdacc7f1e",
    "statusQueryGetUri":"http://localhost:7071/runtime/webhooks/durabletask/instances/13ce7b3e0da8405cb12781acdacc7f1e?taskHub=TestHubName&connection=Storage&code=Jg2Pnt0EJU8OS2yXI7Zn5aBnHldpfGkvkwppeu6F2Xj2AzFuQ-TqjQ==",
    "sendEventPostUri":"http://localhost:7071/runtime/webhooks/durabletask/instances/13ce7b3e0da8405cb12781acdacc7f1e/raiseEvent/{eventName}?taskHub=TestHubName&connection=Storage&code=Jg2Pnt0EJU8OS2yXI7Zn5aBnHldpfGkvkwppeu6F2Xj2AzFuQ-TqjQ=="
    ...
}
```

> Those are the URLs you call to get an update on the progress of the function.

This is not something end-users would see, but you could have a web UI checking for updates and formatting the responses nicely. If you call the `statusQueryGetUri` from your response, you can see the status:

```
# use your own URL from the trigger response
curl "http://localhost:7071/runtime/webhooks/durabletask/instances/xyz?taskHub=TestHubName&connection=Storage&code=abc"
```

The response will include the quote response from the best-priced supplier, something like this:

```
    {
        "name": "QuoteOrchestrator",
        "instanceId": "13ce7b3e0da8405cb12781acdacc7f1e",
        "runtimeStatus": "Completed",
        "input": {
            "QuoteId": "5a9b8cb3-055b-461b-8846-12f6d8f930e2",
            "ProductCode": "P101",
            "Quantity": 32
        },
        "customStatus": null,
        "output": {
            "QuoteId": "5a9b8cb3-055b-461b-8846-12f6d8f930e2",
            "SupplierCode": "SUPPLIER-3",
            "Quote": 256.0
        },
        "createdTime": "2022-11-14T03:56:07Z",
        "lastUpdatedTime": "2022-11-14T03:56:22Z"
    }
```

When you're happy with how it all works, you can deploy to Azure.

## Deploy to Azure

This is the basic setup for your Function App:

```
az group create -n labs-functions-durable-fan-out --tags courselabs=azure -l eastus

az storage account create -g labs-functions-durable-fan-out --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-durable-fan-out  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

**There are no dependencies** - no external services are used for triggers or bindings, so you can go right ahead and deploy:

```
func azure functionapp publish <function-name>
```

Try the function using the public URL. It should work in the same way and you should be able to track the orchestration progress in the _Monitor_ tab for the function in the Portal.

## Lab

The fan-out pattern with a durable function is very powerful, but it's less flexbile than an event-driven pattern with separate functions. How would you on-board a new supplier with this pattern? How does that compare with a traditional pub-sub pattern which didn't use durable functions? And how could you use either pattern to implement a workflow which took the cheapest quote returned within _x_ seconds, to prevent the workflow taking too long to run?

> Stuck? Try my [suggestions](suggestions.md) 
___

## Cleanup

Stop the Azure Storage emulator:

```
docker rm -f azurite
```

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-durable-fan-out
```
