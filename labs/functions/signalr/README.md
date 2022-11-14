# Functions: SignalR Host

SignalR has quite a simple workflow - clients fetch a web page which has a JavaScript function that runs and connects to the SignalR _hub_. The hub manages client connections and is the component which sends messages to all or some of the connections.

Azure Functions can be used to model the whole workflow, providing the HTML page from an HTTP trigger, setting up the SignalR Hub connection and using another trigger to push data out to connected clients. 

In this lab we'll run a simple web app which uses SignalR for timed updates, hosting the whole application in Azure Functions.

## Reference

- [SignalR Service trigger & binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-signalr-service?tabs=in-process&pivots=programming-language-csharp)

- [Distance from the Earth to the Moon](https://spaceplace.nasa.gov/moon-distance/en/)

## Timer pushing to SignalR Service

The scenario is an application which monitors the distance from Earth to the Moon and broadcasts updates to clients over SignalR. Azure Functions will provide the static HTML for the web page, as well as integration with the Azure SignalR Service.

The code is in the `MoonDistance` folder:

- [MoonDistance/Index.cs](/labs/functions/signalr/MoonDistance/Index.cs) - HTTP trigger which returns with the content of the [index.html](/labs/functions/signalr/MoonDistance/content/index.html) file; this is the entrypoint to the web app, the HTML contains JavaScript for the SignalR connection

- [MoonDistance/Negotiate.cs](/labs/functions/signalr/MoonDistance/Negotiate.cs) - handles the SignalR connection setup, this is the method called when a new client loads the web app; we don't need any connection details in the code because the `SignalRConnectionInfo` is an input binding which provides that for us

- [MoonDistance/Broadcast.cs](/labs/functions/signalr/MoonDistance/Broadcast.cs) - timed trigger which fires every 10 seconds and sends a SignalR update with the current (fake) distance to the Moon; binds to the same SignalR Service as the negotiate function, so will send to all connected clients.


SignalR Service can work in different modes, and for Azure Functions we need to use the [Serverless mode](https://learn.microsoft.com/en-us/azure/azure-signalr/concept-service-mode#serverless-mode). 

## Test the function locally

There is a [SignalR Service emulator](https://github.com/Azure/azure-signalr/blob/dev/docs/emulator.md) which you would use if you do a lot of work with SignalR, but we'll spin up a real SignalR Service to use:

```
az group create -n labs-functions-signalr --tags courselabs=azure 

az signalr create -g labs-functions-signalr --service-mode Serverless --sku Free_F1  -n <signalr-name>
```

While that's creating, run the local Azure Storage emulator:

```
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

And create the file `labs/functions/signalr/MoonDistance/local.settings.json` **with your own SignalR connection string**:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "AzureSignalRConnectionString" : "<signalr-connection-string>"
    }
}
```

Run the function locally:

```
cd labs/functions/signalr/MoonDistance

func start
```

Open the index URL in your browser: http://localhost:7071/api/index

You should get regular updates from the Moon.


## Deploy to Azure

We already have the resource group and SignalR Service, so we just need the Function App Setup:

```
az storage account create -g labs-functions-signalr --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-signalr  --runtime dotnet --functions-version 4 --consumption-plan-location eastus  --storage-account <sa-name> -n <function-name> 
```

There are no more services to create, but you do need to wire everything up. Function Apps can run with a Managed Identity, but the SignalR bindings don't currently support that, so we need to use a SignalR connection string with a key:

- the connection string for SignalR needs to be set in the Function appsetting `AzureSignalRConnectionString`

Then you can deploy the function:

```
func azure functionapp publish <function-name>
```

Browse to your Azure Function URL: `https://<function-name>.azurewebsites.net/api/index` and you should see the updates.

## Lab

Every ten seconds is quite frequent for a timed trigger. We may want that in dev when we're testing the function, but in Azure we probably want to run at a different schedule. The schedule is currently hard-coded in the timer trigger attribute - how could we make it more flexible?

> Stuck? Try [suggestions](suggestions.md).

___

## Cleanup

Stop the Azure Storage emulator:

```
docker rm -f azurite
```

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-signalr
```
