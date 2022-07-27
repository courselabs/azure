# Azure Resource Manager templates

Repeatable deployments, templated with variables; versioned in scm

## Reference

- [Azure Resource Manager overview](https://docs.microsoft.com/en-gb/azure/azure-resource-manager/management/overview)

- [ARM Quickstart template gallery](https://azure.microsoft.com/en-gb/resources/templates/) and [GitHub repo](https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts)

- [Template schema reference](https://docs.microsoft.com/en-us/azure/templates/)

## ARM template JSON

- parameters, with defaults
- variables, with functions
- resources, specified with variables & parameters

```
    "resources": [
      {
        "type": "Microsoft.Storage/storageAccounts",
        "apiVersion": "2019-06-01",
        "name": "[parameters('storageAccountName')]",
        "location": "[parameters('location')]",
        "sku": {
          "name": "[variables('storageSku')]"
        },
        "kind": "StorageV2",
        "properties": {
          "supportsHttpsTrafficOnly": true
        }
      }
    ]
```

## Desired-state deployment

Create a resource group:

```
az group create -n labs-arm  --location westeurope
```

Check the parameters in labs/arm/storage-account/azuredeploy.json - t


```
az deployment group create --help
```

📋  Deploy

Use the template - this will prompt you for parameter values:

```
az deployment group create --name storage-account -g labs-arm  --template-file labs/arm/storage-account/azuredeploy.json
```

Or supply the values to the deployment command:

```
az deployment group create --name storage-account -g labs-arm  --template-file labs/arm/storage-account/azuredeploy.json  --parameters storageAccountName=<unique-name>
```

> Check rg in portal

You can check deployments with the CLI, this will print the basic details:

```
az deployment group list -g labs-arm -o table
```

ARM deployments are repeatable - you should get the same result however many times you run it, whatever the current state is.

The `what-if` flag tells you what the result of the deployment would be, without actually making any changes - be sure to use the same parameter value(s)

```
az deployment group create --name storage-account -g labs-arm  --template-file labs/arm/storage-account/azuredeploy.json --what-if --parameters storageAccountName=<storage-account-name>
```

> You'll see the storage account listed as "no change"

ARM deployments are useful for identifying and fixing _drift_ - where the deployment is manually changed and the update isn't reflected in the template.

📋 Change your storage account using `storage account upate` and set the SKU to be `Standard_GRS`. After that run the what-if deployment again.

az storage account update --help

az storage account update -g labs-arm --sku Standard_GRS -n <storage-account-name>

Run the what-if comand with the same parameter values:

```
az deployment group create --name storage-account -g labs-arm  --template-file labs/arm/storage-account/azuredeploy.json --what-if --parameters storageAccountName=<storage-account-name>
```

> Now the output tells you the deployment would change the SKU back to `Standard_LRS`


## Dynamic values in ARM templates

ARM templates are meant to be repeatable but they may not be - if they were authored with dynamic settings then those will be different for each deployment and the template won't be idempotent.

Check out this template which comes from the Azure Quickstart repo on GitHub:

- labs/arm/vm-simple-linux/azuredeploy.json - defines a VM  and all the associated resources: VNet, subnet, PIP, NSG and NIC

📋 Can you find the setting which stops this template being repeatable?

Inside the NIC resource you'll see the IP configuration settings:

```
"properties": {
        "ipConfigurations": [
          {
            "name": "ipconfig1",
            "properties": {
              "subnet": {
                "id": "[resourceId('Microsoft.Network/virtualNetworks/subnets', parameters('virtualNetworkName'), parameters('subnetName'))]"
              },
              "privateIPAllocationMethod": "Dynamic"
```

The allocation method for the private IP address - within the subnet - is set to _Dynamic_. That means a different address could be used each time..

Dynamic values make the template difficult to use for upgrades.

📋 Deploy the VM template to your resource group - use the parameters file in the folder to provide the Linux user name, but set the password and DNS name in the deployment command:

```
az deployment group create --name vm-simple-linux -g labs-arm2  --template-file labs/arm/vm-simple-linux/azuredeploy.json  --parameters @labs/arm/vm-simple-linux/azuredeploy.parameters.json adminPasswordOrKey='ssdfDSDS7777**' dnsLabelPrefix=labs-arm-es2
```

When the deployment completes, run the command again with the `--what-if` flag - does it tell you there are no changes?

> You'll see the deployment wants to modify the IP address because the resource has an actual IP address now, which doesn't match the template spec


## Lab

Change VM spec to use static IP address `10.1.0.102` in the JSON template. Create a new resource group and deploy the new template - verify that the deployment is repeatable.

## Cleanup

You can delete an ARM deployment with the CLI:

```
az deployment group delete -g labs-arm -n storage-account
```

** But that only deletes the deployment metadata, not the actual resources **

To clean up for real we need to delete the Resource Groups:

```
az group delete -n labs-arm 

az group delete -n labs-arm-lab
```

