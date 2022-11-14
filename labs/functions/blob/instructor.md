
Vars:

```
$saName='courselabsazes2210'
$faName='courselabsazes'
```

Setup:

```
az group create -n labs-functions-blob --tags courselabs=azure -l eastus

az storage account create -g labs-functions-blob --sku Standard_LRS -l eastus -n $saName

az functionapp create -g labs-functions-blob  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account $saName -n $faName

func azure functionapp publish $faName
```

Reqs:

sa + sql

```
az storage account create -g labs-functions-blob --sku Standard_LRS -l eastus -n "$($saName)in"

  az sql server create `
    -l eastus -g labs-functions-blob -n labsfuncblobes `
    -u labs `
    -p $env:LABPWD

  az sql db create `
    -g labs-functions-blob -n func `
    -s labsfuncblobes
```

sql firewall:

```
$fnAppIps=$(az functionapp show -g labs-functions-timer -n $faName --query possibleOutboundIpAddresses -o tsv).Split(',')

# TODO - use vnet integration instead   
foreach ($appServiceIp in $fnAppIps) {
    echo "Adding firewall rule for app service IP $appServiceIp"
    az sql server firewall-rule create `
    -g labs-functions-timer -s labsfuncblobes -n "appservice-$appServiceIp" `
    --start-ip-address $appServiceIp --end-ip-address $appServiceIp
}
```

appsettings

```
$cs=$(az storage account show-connection-string -o tsv -g labs-functions-blob --name "$($saName)in")

$connectionStringTemplate=$(az sql db show-connection-string --server labsfuncblobes --client ado.net -o tsv)

$connectionString=$connectionStringTemplate.Replace('<databasename>', 'func').Replace('<username>', 'labs').Replace('<password>', $env:LABPWD)

az functionapp config appsettings set -g labs-functions-blob -n $faName --settings "UploadSqlServerConnectionString=$connectionString" "UploadInputStorageConnectionString=$cs"
```

