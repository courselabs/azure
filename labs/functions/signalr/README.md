
run emulator

create a **serverless** signalr service


get connection string


deploy to func app - use MI & managed connection string

## Deploy to Azure

Setup:

```
az group create -n labs-functions-signalr --tags courselabs=azure -l eastus

az storage account create -g labs-functions-signalr --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-signalr  --runtime dotnet --functions-version 4 --consumption-plan-location eastus  --storage-account <sa-name> -n <function-name> 
```

Pre-reqs:

- serverless signalr service

- signalr connection string


deploy:

```
func azure functionapp publish <function-name>
```