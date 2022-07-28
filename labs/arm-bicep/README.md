# ARM Templates with Bicep

DSL, generates JSON - more manageable, easier to maintain

## Reference

- [Bicep overview](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview?tabs=bicep)

- [ARM Quickstart template gallery](https://azure.microsoft.com/en-gb/resources/templates/) and [GitHub repo](https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts)

- [Template schema reference](https://docs.microsoft.com/en-us/azure/templates/)



## Bicep syntax & deployments

```
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
```

- https://docs.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts?tabs=bicep

- simple name reference for parameters and variables 

Bicep is a much friendlier language to use - you can deploy Bicep files directly or you can generate ARM JSON files from the Bicep. You don't need any additional tools to deploy Bicep files.

📋 Create a resource group `labs-arm-bicep` and use a `deployment group create` command to deploy the Bicep file in `labs/arm-bicep/storage-account/main.bicep`.

```
az group create -n labs-arm-bicep --location westeurope
```

This will request parameter values from the CLI:

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/storage-account/main.bicep
```

Or supply the values in the command:

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/storage-account/main.bicep --parameters storageAccountName=<unique-name>
```

> The output from the deployment command is the same for JSON and Bicep specs.

What-if support is there too - run this command with a differnet SKU parameter but the same account name, and the ourput will tell you what would change:

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/storage-account/main.bicep --what-if --parameters storageSku=Standard_GRS storageAccountName=<unique-name> 
```

## Using the Bicep tools

Bicep is the preferred way of using ARM, but it's more recent than the JSON templates - so lots of projects will still use JSON. With the Bicep tools you can generate Bicep files from JSON specs and vice versa, which means you can define your infrastructure with more manageable Biceps, but continue using the existing ARM workflow.

Install from the CLI:

```
az bicep install
```

> If that doesn't work for you try another [Bicep installation option](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/install#deployment-environment)

This ARM spec is for the same Linux VM in the [ARM lab]() - all the resources are defined in JSON format:

- labs/arm-bicep/vm-simple-linux/azuredeploy.json

Use the Bicep CLI to generate a Bicep file from the JSON:

```
az bicep --help

# the `decompile` command generates Bicep from ARM:
az bicep decompile --help 

az bicep decompile -f labs/arm-bicep/vm-simple-linux/azuredeploy.json
```

Run a what-if deployment from the generated Bicep file to check that it is valid

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/vm-simple-linux/azuredeploy.bicep --what-if --parameters adminUsername=linuxuser adminPasswordOrKey=<strong-password>
```

> You'll see output for all the resources to be created, and you may see warnings about the generated template.

📋 Fix the warnings in the Bicep file and update the NIC to use the static IP `10.1.0.103` - then deploy the template.

Sample file: labs/arm-bicep/vm-simple-linux/azuredeploy-updated.bicep

- sets the same _privateIP_ values we used in the ARM lab

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/vm-simple-linux/azuredeploy-updated.bicep --parameters adminUsername=linuxuser adminPasswordOrKey=<strong-password>
```

You should find the Bicep file much easier to navigate and edit.

## Evolving infrastructure specs

Bicep templates are typically meant to describe all the resources in the resource group. 

The default deployment mode for ARM is _incremental_ which means any new resources in the template get added, any matching ones are left as they are, and anything extra in the Resource Group (not described in the template) is left as-is.

- labs/arm-bicep/vm-and-sql-db/main.bicep - adds a SQL Server and database spec to the existing Linux VM template

The resource identifiers have been tidied up from the generated Bicep, but the specs are the same. Run a what-if deployment and you'll see three new resources to be added and no changes to the existing resources:

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/vm-and-sql-db/main.bicep  --parameters adminUsername=linuxuser adminPasswordOrKey='dfFSFSF3232**' sqlAdminPassword='dfFSFSF3232**' --what-if
```


## Lab

TODO
