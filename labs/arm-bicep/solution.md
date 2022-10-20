

Added parameters with allowed lists:

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

And updated SQL Database resource:

```
  sku: {
    name: databaseSku
    tier: databaseSku
  }
  properties: {
    sampleName: databaseSampleSchema
  }
```