# Azure Functions

Functions are a _serverless_ compute platform - you supply the code and you don't need to provision infrastructure or manage scale.

## References

- [Azure Functions overview](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview)

- [Functions developer guide](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference?source=recommendations&tabs=blob)

- [`az functionapp` commands]

## Explore Azure Functions

portal, search 'function', select 'function app' & create:

- name is a DNS prefix
- deployment choice is Code or Docker Container
- for Code choose runtime & OS
- consumption can be Serverless, Premium or shared App Service Plan
- link to a Storage Account is required

We'll switch to the CLI to deploy a function, first we need to install the Azure FUnctions Core Tools:

```
# Windows with chocolatey:
choco install azure-functions-core-tools

# macOS with brew:
brew tap azure/functions
brew install azure-functions-core-tools@4
```

> There are [other installers available](https://github.com/Azure/azure-functions-core-tools) - be sure to install the v4 option for your OS.

Check your installation:

```
func --version
```


## Run Functions locally

- labs/functions/calendar/HttpTime.cs

local emulator

```
cd labs/functions/calendar

func start
```

> .NET build output, listening on localhost

```
curl http://localhost:7071/api/HttpTime
```

Short feedback loop, test functionality

That's it :)

## Deploy to Azure Functions

Deploy - needs RG and Storage Account:

```
# find location near which supports functions
az functionapp list-consumption-locations -o table

az group create -n labs-functions --tags courselabs=azure -l eastus

az storage account create -g labs-functions --sku Standard_LRS -l eastus -n <sa-name>

```

Create a Function App:

```
az functionapp create --help
```

```
az functionapp create -g labs-functions  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

Check in the Portal - you've got an app service plan with no scale up or scale out options, this is the serverless (consumption) plan. Under _apps_ you've got the function app:

- public URL
- OS defaults to windows
- UX very similar to web app

Deploy from the local tools:

```
func azure functionapp publish <function-name>
```

> Compiles locally and uploads

Output includes public URL for function:

```
curl <public-url>/HttpTime
```

Check the storage account - blob containers are locked, but file storage is available. This is the root of the app; can you find the application logs?

## Add more functions

```
# still in the CalenderProj folder:
pwd

cp ../update/*.cs .

func start
```

> Now three HTTP functions

Test:

```
curl http://localhost:7071/api/HttpDay
```

Deploy to Azure:

```
func azure functionapp publish <function-name>
```

Try:

```
curl <public-url>/HttpTime

curl <public-url>/HttpDate
```

Check in the Portal - you still have one Function App in the Plan. Open it and you can see the individual _Functions_. Open one and check the menu options - _Integration_ shows you the triggers, inputs and outputs; _Monitor_ shows you the recent runs and results, click one to see the logs.


Try the last one:

```
curl -i <public-url>/HttpDay
```

> Not authorized

Back in the Portal open _HttpDay_ function click _Get function URL_ - output includes a key, also shown in _Function Keys_. This fn is flagged as needing auth (others are anonymous).

```
curl https://courselabsazes.azurewebsites.net/api/HttpDay?code=WNGtEqCqMqF-whsqFzfaCbPOgXqVS1lZ9eepgHIXvObgAzFuJ2bGVA==
```

## Lab

Create a new function using the `func new` command line - start from an empty directory and choose Powershell as the runtime. Pick the HTTP trigger. Run the function locally - does it work without any code changes? Publish the function to Azure. Can you use your existing function app?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions
```