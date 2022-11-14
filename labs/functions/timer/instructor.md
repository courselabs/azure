
Vars:

```
$saName='courselabsazes2210'
$faName='courselabsazes'
```

Setup:

```
az group create -n labs-functions-timer --tags courselabs=azure -l eastus

az storage account create -g labs-functions-timer --sku Standard_LRS -l eastus -n $faName

az functionapp create -g labs-functions-timer  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account $faName -n $faName

func azure functionapp publish $faName
```

Reqs:

```
az storage account create -g labs-functions-timer --sku Standard_LRS -l eastus -n hboutputstoragees

$cs=$(az storage account show-connection-string -o tsv -g labs-functions-timer --name hboutputstoragees)

az functionapp config appsettings set -g labs-functions-timer -n $faName --settings "HeartbeatOutputStorageConnectionString=$cs"
```

