
```
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite

az group create -n labs-functions-signalr --tags courselabs=azure 

az signalr create -g labs-functions-signalr --service-mode Serverless --sku Free_F1  -n <signalr-name>

az signalr key list -g labs-functions-signalr --query primaryConnectionString -n $env:LABY
```

MI - DOESN'WORK WITH THE FN SIGR LIB:
```
$id=$(az functionapp identity assign -g labs-functions-signalr -n $env:LABZ --query principalId -o tsv)

$signalr=$(az signalr show -g labs-functions-signalr --query id -n $env:LABY -o tsv)

az role assignment create  --role 'SignalR App Server' --assignee-object-id $id --scope $signalr

$mics="Endpoint=https://$($env:LABY).service.signalr.net;AuthType=azure.msi;Version=1.0;"

az functionapp config appsettings set -g labs-functions-signalr -n $env:LABZ --settings "AzureSignalRConnectionString=$mics" 
``