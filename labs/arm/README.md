# Azure Resource Manager JSON templates

The Azure CLI is a great tool for exploring and automating deployments - but it's an imperative approach. If you use it in scripts you'll need to add lots of checks to make sure you're not trying to create resources which already exist. The alternative is the declarative approach - where you describe what you want the end result to be and the tooling works out if it needs to creat or update resources.

In this lab we'll use Azure Resource Manager (ARM) templates for deployment. These are JSON models of the desired state of your resources, which can live in source control.

## Reference

- [Azure Resource Manager overview](https://docs.microsoft.com/en-gb/azure/azure-resource-manager/management/overview)

- [ARM Quickstart template gallery](https://azure.microsoft.com/en-gb/resources/templates/) and [GitHub repo](https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts)

- [Template schema reference](https://docs.microsoft.com/en-us/azure/templates/)

## ARM template JSON

ARM templates are powerful, but they're not easy to work with. Here's a simple template:

- [storage-account/azuredeploy.json](./storage-account/azuredeploy.json)

That template will create a Storage Account. It has multiple blocks in the JSON:

- parameters, with values that can be changed for each deployment
- variables, which set values to use in the rest of the template
- resources, which declare the actual resources to create - using the variables & parameters

This snippet defines the Storage Account to create:

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

- the `type` and `apiVersion` state the resource we're defining (see [storage accounts in ARM](https://docs.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts?tabs=json))
- the name and location of the Storage Account are read from parameters
- the SKU is read from a veraible
- other properties are set in the resource block

You can share this template, and whenever it's deployed you can be sure it will create a v2 storage account, using the Standard LRS SKU and set for HTTPS access only.

## Desired-state deployment

ARM templates can be deployed using the CLI. You always deploy a template into an existing Resource Group.

Strat by creating a resource group:

```
az group create -n labs-arm --tags courselabs=azure --location westeurope
```

📋 Use the `deployment group create` command to deploy the ARM template in `labs/arm/storage-account/azuredeploy.json`

<details>
  <summary>Not sure how?</summary>

```
# print the help text:
az deployment group create --help
```

You can deploy the template without any extra settings, and the CLI will prompt you to supply parameter values:

```
az deployment group create --name storage-account -g labs-arm  --template-file labs/arm/storage-account/azuredeploy.json
```

Or you can supply parameter values to the deployment command:

```
az deployment group create --name storage-account -g labs-arm  --template-file labs/arm/storage-account/azuredeploy.json  --parameters storageAccountName=<unique-name>
```

</details><br/>

All parameters are required to have values, but the location parameter has a default set in the template, so only the storage account name needs to be set.

> Check the Resource Group in the Portal while the deployment is running.

You can also check deployments with the CLI, this will print the basic details:

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

<details>
  <summary>Not sure how?</summary>

Print the help text:

```
az storage account update --help
```

Change the SKU:

```
az storage account update -g labs-arm --sku Standard_GRS -n <storage-account-name>
```

Run the what-if comand with the same parameter values:

```
az deployment group create --name storage-account -g labs-arm  --template-file labs/arm/storage-account/azuredeploy.json --what-if --parameters storageAccountName=<storage-account-name>
```

</details><br/>

> Now the output tells you the deployment would change the SKU back to `Standard_LRS`

This is very useful for auditing running deployments and checking they haven't been manually changed.

## Dynamic values in ARM templates

ARM templates are meant to be repeatable but they may not be. Some Azure settings are dynamic, so they would change with each deployment. If an ARM template contains settings like that then it won't be idempotent.

Check out this template which comes from the Azure Quickstart repo on GitHub:

- [vm-simple-linux/azuredeploy.json](/labs/arm/vm-simple-linux/azuredeploy.json) - defines a VM  and all the associated resources: VNet, subnet, PIP, NSG and NIC

📋 Can you find the setting which stops this template being repeatable?

<details>
  <summary>Not sure?</summary>

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

The allocation method for the private IP address - within the subnet - is set to _Dynamic_. That means a different address could be used each time.

</details><br/>

Dynamic values make the template difficult to use for upgrades.

📋 Deploy the VM template to your resource group - use the parameters file in the folder to provide the Linux user name, but set the password and DNS name in the deployment command:

```
az deployment group create --name vm-simple-linux -g labs-arm  --template-file labs/arm/vm-simple-linux/azuredeploy.json  --parameters @labs/arm/vm-simple-linux/azuredeploy.parameters.json adminPasswordOrKey='<strong-password>' dnsLabelPrefix=<unique-dns-name>
```

When the deployment completes, run the command again with the `--what-if` flag - does it tell you there are no changes?

> You'll see the deployment wants to modify the IP address because the resource has an actual IP address now, which doesn't match the template spec


## Lab

Change the VM spec to use a static IP address `10.1.0.102` in the JSON template. Create a new resource group and deploy the new template - verify that the deployment is repeatable.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

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

