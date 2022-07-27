param location string = 'westeurope'
param storageAccountName string
param storageSku string = 'Standard_LRS'

var httpsOnly = true

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-06-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageSku
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: httpsOnly
  }
}
