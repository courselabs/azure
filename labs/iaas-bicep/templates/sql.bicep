
param location string = resourceGroup().location
var common = loadJsonContent('vars.json')
param sqlServerName string = uniqueString('sql', resourceGroup().id)
param sqlDBName string = 'signup-db'
param sqlAdminUser string = 'sqladmin'

@secure()
param sqlAdminPassword string

resource vnet 'Microsoft.Network/virtualNetworks@2021-05-01' existing = {
  name: common.vnetName
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2021-05-01' existing = {
  parent: vnet
  name: common.subnetName
}

resource sqlServer 'Microsoft.Sql/servers@2021-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminUser
    administratorLoginPassword: sqlAdminPassword
  }
}

resource sqlServerVnet 'Microsoft.Sql/servers/virtualNetworkRules@2021-11-01-preview' = {
  name: 'sqlServerVnetRule1'
  parent: sqlServer
  properties: {
    ignoreMissingVnetServiceEndpoint: true
    virtualNetworkSubnetId: subnet.id
  }
}

resource sqlDB 'Microsoft.Sql/servers/databases@2021-08-01-preview' = {
  parent: sqlServer
  name: sqlDBName
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
}
