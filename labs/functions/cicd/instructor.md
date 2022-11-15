
core: 

```
$rg='labs-functions-cicd'
$sb='lbfncicdes221122'
$topic='QuoteRequestTopic'
$sa='lbfncicdes221122'
$fn=$env:LABNAME2
$fnSa=$env:LABNAME2

az group create -n $rg --tags courselabs=azure -l westeurope
```

serice bus:

```
az servicebus namespace create -g $rg --location westeurope --sku Basic -n $sb


az servicebus queue create --max-size 1024 --default-message-time-to-live P0DT1H0M0S -n HeartbeatCreated -g $rg --namespace-name $sb
```

table storage

```
az storage account create -g $rg --sku Standard_LRS -l westeurope -n $sa

az storage table create -n heartbeats --account-name $sa
```


Setup:

```
az storage account create -g $rg --sku Standard_LRS -l westeurope -n $fnSa

az functionapp create -g $rg --runtime dotnet --functions-version 4 --consumption-plan-location westeurope --storage-account $fnSa -n $fn

$sbConnectionString=$(az servicebus namespace authorization-rule keys list -n RootManageSharedAccessKey -g $rg  --query primaryConnectionString -o tsv --namespace-name $sb)

$saConnectionString=$(az storage account show-connection-string -o tsv -g $rg --name $sa)

az functionapp config appsettings set -g $rg -n $fn --settings "StorageConnectionString=$saConnectionString" "ServiceBusConnectionString=$sbConnectionString"  

func azure functionapp publish $fn
```

