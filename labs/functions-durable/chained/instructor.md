
core: 

```
$rg='labs-functions-durable-chained'
$sb='lbfnsbes22112'
$sa='lbfnsbes22112'
$fn='lbfnsbes22112'
$fnSa='salbfnsbes22112'

az group create -n $rg --tags courselabs=azure -l westeurope
```

serice bus:

```
az servicebus namespace create -g $rg --location westeurope --sku Standard -n $sb

az servicebus queue create --max-size 1024 --default-message-time-to-live P0DT1H0M0S -n HeartbeatCreated -g $rg --namespace-name $sb
```

table storage

```
az storage account create -g $rg --sku Standard_LRS -l westeurope -n $sa

## az storage table create -n quotes --account-name $sa
```


Setup:

```
az storage account create -g $rg --sku Standard_LRS -l westeurope -n $fnSa

az functionapp create -g $rg --runtime dotnet --functions-version 4 --consumption-plan-location westeurope --storage-account $fnSa -n $fn

$sbConnectionString=$(az servicebus namespace authorization-rule keys list -n RootManageSharedAccessKey -g $rg  --query primaryConnectionString -o tsv --namespace-name $sb)

$saConnectionString=$(az storage account show-connection-string -o tsv -g $rg --name $sa)

az functionapp config appsettings set -g $rg -n $fn --settings "ServiceBusConnectionString=$sbConnectionString" "StorageConnectionString=$saConnectionString"

func azure functionapp publish $fn
```

