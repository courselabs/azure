param location string = resourceGroup().location
var common = loadJsonContent('vars.json')
param vmName string = uniqueString('vm', resourceGroup().id)
param dnsLabelPrefix string = toLower('signup-${vmName}')
param vmSize string = 'Standard_D2s_v5'
param adminUsername string = 'vmadm'
param sqlServerName string
@secure()
param sqlPassword string
@secure()
param adminPassword string

var publicIPAddressName = '${vmName}-pip'
var networkInterfaceName = '${vmName}-nic'
var osDiskType = 'Standard_LRS'
var windowsServerSku = '2022-datacenter-core-g2'
var privateIpAddress = '10.1.0.103'

resource vnet 'Microsoft.Network/virtualNetworks@2021-05-01' existing = {
  name: common.vnetName
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2021-05-01' existing = {
  parent: vnet
  name: common.subnetName
}

resource networkSecurityGroup 'Microsoft.Network/networkSecurityGroups@2021-05-01' existing = {
  name: common.nsgName
}

resource networkInterface 'Microsoft.Network/networkInterfaces@2021-05-01' = {
  name: networkInterfaceName
  location: location
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          subnet: {
            id: subnet.id
          }
          privateIPAllocationMethod: 'Static'
          privateIPAddressVersion: 'IPv4'
          privateIPAddress: privateIpAddress
          publicIPAddress: {
            id: publicIPAddress.id
          }
        }
      }
    ]
    networkSecurityGroup: {
      id: networkSecurityGroup.id
    }
  }
}

resource publicIPAddress 'Microsoft.Network/publicIPAddresses@2021-05-01' = {
  name: publicIPAddressName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Regional'
  }
  properties: {
    publicIPAllocationMethod: 'Dynamic'
    publicIPAddressVersion: 'IPv4'
    dnsSettings: {
      domainNameLabel: dnsLabelPrefix
    }
    idleTimeoutInMinutes: 4
  }
}

resource vm 'Microsoft.Compute/virtualMachines@2021-11-01' = {
  name: vmName
  location: location
  properties: {
    hardwareProfile: {
      vmSize: vmSize
    }
    storageProfile: {
      osDisk: {
        createOption: 'FromImage'
        managedDisk: {
          storageAccountType: osDiskType
        }
      }
      imageReference: {
        publisher: 'MicrosoftWindowsServer'
        offer: 'WindowsServer'
        sku: windowsServerSku
        version: 'latest'
      }
    }
    networkProfile: {
      networkInterfaces: [
        {
          id: networkInterface.id
        }
      ]
    }
    osProfile: {
      computerName: vmName
      adminUsername: adminUsername
      adminPassword: adminPassword
    }
  }
}

resource vmRunCommand 'Microsoft.Compute/virtualMachines/runCommands@2022-03-01' = {
  name: 'vmSetup'
  location: location
  parent: vm
  properties: {
    asyncExecution: false
    protectedParameters: [
      {
        name: 'sqlServer'
        value: sqlServerName
      }
      {
        name: 'sqlPassword'
        value: sqlPassword
      }
    ]
    source: {
      commandId: 'RunPowerShellScript'
      scriptUri: 'https://TODO-GITHUB_URL/vm-setup.ps1'
    }
    timeoutInSeconds: 600
  }
}
