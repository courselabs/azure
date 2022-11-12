
Vars:

```
$rg='labs-functions-cosmos'
$fnsa='labscosmoses'
$fn='labscosmoses'
$cosmosAccount='labsfncosmoses'
```

Setup:

```
az group create -n $rg --tags courselabs=azure -l eastus

az storage account create -g $rg --sku Standard_LRS -l eastus -n $fnsa

az functionapp create -g $rg  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account $fnsa -n $fn
```

Reqs:

```

# cosmos

az cosmosdb create -g $rg  --enable-public-network --kind GlobalDocumentDB --locations regionName=eastus -n $cosmosAccount

az cosmosdb sql database create --name Test -g $rg  --account-name $cosmosAccount

az cosmosdb sql container create -g $rg -a $cosmosAccount -d Test -n posts --partition-key-path "/id"

az cosmosdb sql database create --name Prod -g $rg  --account-name $cosmosAccount

$cs=$(az cosmosdb keys list --type connection-strings -g $rg  --query "connectionStrings[?description==``Primary SQL Connection String``].connectionString" -o tsv -n $cosmosAccount)

az functionapp config appsettings set -g $rg -n $fn --settings "CosmosDbConnectionString=$cs" 
```

Deploy:

```
func azure functionapp publish $fn
```