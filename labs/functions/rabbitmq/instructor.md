
Vars:

```
$rg='labs-functions-rabbitmq'
$sa='labsrabbitmqes'
$fn='labsrabbitmqes'
$fnsa='fnlabsrabbitmqes'
```

Setup:

```
az group create -n $rg --tags courselabs=azure -l eastus

az storage account create -g $rg --sku Standard_LRS -l eastus -n $fnsa

az functionapp create -g $rg  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account $fnsa -n $fn
```

Reqs:

```
# rabbitmq vm

# Publisher bitnami
# Offer rabbitmq
# Plan rabbitmq

az storage account create -g $rg  --sku Standard_LRS -l eastus -n $sa

$cs=$(az storage account show-connection-string -o tsv -g $rg  --name $sa)

az functionapp config appsettings set -g $rg -n $fn --settings "CustomerOutputStorageConnectionString=$cs" "InputRabbitMQConnectionString=amqp://user:pwd@pi:5672"
```

Deploy:

```
func azure functionapp publish $fn
```