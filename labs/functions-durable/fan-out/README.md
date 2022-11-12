## Durable Functions: Fan-Out

Invoke multiple activities in parallel; wait for all (or some) to complete and work on the data

## Reference

- [Durable Functions for Fan-Out](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp#human)


## Pre-req

# Reference



func init QuoteEngine --dotnet 

cd QuoteEngine

func new --name QuoteOrchestrator --template "DurableFunctionsOrchestration"

dotnet add package Microsoft.Azure.WebJobs.Extensions.DurableTask --version 2.8.1

dotnet add package  Microsoft.Azure.WebJobs.Extensions.Twilio --version 3.0.2


## Run locally

- local.settings.json

Start the Azure Storage emulator:

```
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

Run (maybe change the timer to every 1 minute)

```
func start
```

Test:

```
curl http://localhost:7071/api/HttpOrchestratorStart
```
"
Check logs. Curl the URL in `statusQueryGetUri`
