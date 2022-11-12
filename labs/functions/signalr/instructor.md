
```
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite

az group create -n labs-functions-signalr --tags courselabs=azure 

az signalr create -g labs-functions-signalr --service-mode Serverless --sku Free_F1  -n <signalr-name>

az signalr key list -g labs-functions-signalr --query primaryConnectionString -n $env:LABNAME
```

MI:
```
az functionapp identity assign -g labs-functions-signalr -n <fn-name>

az signalr show -g labs-functions-signalr --query id -n <signalr-name>

az role assignment create  --role 'SignalR App Server' --assignee-object-id <principalId> --scope "<signalr-id>"
``