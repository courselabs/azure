# Lab Solution

See the sample Bicep file here:

- [lab/main.bicep](/labs/arm-bicep/lab/main.bicep)

That adds two new parameters, each with a list of allowed values:

```
@allowed([
  'Basic'
  'Standard'
])
param databaseSku string

@allowed([
  'AdventureWorksLT'
  'WideWorldImportersStd'
])
param databaseSampleSchema string
```

And updates the SQL Database resource, to load the Sku from the parameter and to add a new properties block with the sample schema name:

```
  sku: {
    name: databaseSku
    tier: databaseSku
  }
  properties: {
    sampleName: databaseSampleSchema
  }
```

You can deploy the template into a new Resource Group:

```
az group create -n labs-arm-bicep-lab  --tags courselabs=azure --location westeurope

# this will ask you to choose the SKU & sample schema:
az deployment group create -g labs-arm-bicep-lab --template-file labs/arm-bicep/lab/main.bicep  --parameters adminUsername=linuxuser adminPasswordOrKey=<strong-password> sqlAdminPassword=<strong-password> 
```

Browse to your SQL Database in the Portal, open Query Editor and you should see the schema and data deployed for your sample database.