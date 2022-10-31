# ARM Templates with Bicep

The concepts behind ARM templates are important ones - infrastructure as code, parameterized deployments, desired-state delivery. But the JSON format is difficult to work with, especially for larger applications with multiple resources.

The evolution of ARM is a new tool called Bicep, which uses a custom language to define resources in a simpler and more manageable template.

## Reference

- [Bicep overview](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/overview?tabs=bicep)

- [ARM Quickstart template gallery](https://azure.microsoft.com/en-gb/resources/templates/) and [GitHub repo](https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts)

- [Template schema reference](https://docs.microsoft.com/en-us/azure/templates/)


## Bicep syntax & deployments

Here's a snippet of Bicep for defining a [Storage Account](https://docs.microsoft.com/en-us/azure/templates/microsoft.storage/storageaccounts?tabs=bicep):

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

The resource type specifies the version of the schema its using, but the rest of the template [storage-account/main.bicep](/labs/arm-bicep/storage-account/main.bicep) is much more readable than the JSON alternative:

- the syntax is cleaner - field names and values don't need to be quoted and there is less indentation
- paramaters and variables have simple names (`location` and `storageSku`)
- templates can include comments

Bicep is a much friendlier language to use. You can deploy Bicep files directly or you can generate ARM JSON files from the Bicep. You don't need any additional tools to deploy Bicep files.

📋 Create a resource group `labs-arm-bicep` and use a `deployment group create` command to deploy the Bicep file in `labs/arm-bicep/storage-account/main.bicep`.

<details>
  <summary>Not sure how?</summary>

```
az group create -n labs-arm-bicep  --tags courselabs=azure --location westeurope
```

This will request parameter values from the CLI:

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/storage-account/main.bicep
```

Or supply the values in the command:

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/storage-account/main.bicep --parameters storageAccountName=<unique-name>
```

</details><br/>

> The output from the deployment command is the same for JSON and Bicep specs.

What-if support is there too - run this command with a different SKU parameter but the same account name, and the output will tell you what would change:

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/storage-account/main.bicep --what-if --parameters storageSku=Standard_GRS storageAccountName=<unique-name> 
```

## Using the Bicep tools

Bicep is the preferred way of using ARM, but it's more recent than the JSON templates - so lots of projects will still use JSON. With the Bicep tools you can generate Bicep files from JSON specs and vice versa, which means you can define your infrastructure with more manageable Biceps, but continue using the existing ARM workflow.

You can install Bicep tools directly from the CLI:

```
az bicep install

# the bicep commands have their own help text:
az bicep --help
```

> If that doesn't work for you try another [Bicep installation option](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/install#deployment-environment)

This ARM spec is for the same Linux VM in the [ARM lab](/labs/arm/README.md) - all the resources are defined in JSON format:

- [vm-simple-linux/azuredeploy.json](/labs/arm-bicep/vm-simple-linux/azuredeploy.json)

📋 Use the Bicep CLI to generate a Bicep file from the JSON.

<details>
  <summary>Not sure how?</summary>

The `decompile` command generates Bicep from ARM:

```
az bicep decompile --help 

az bicep decompile -f labs/arm-bicep/vm-simple-linux/azuredeploy.json
```

</details><br/>

You should have a file generated in `labs/arm-bicep/vm-simple-linux/azuredeploy.bicep`. Check that file and you'll see:

- there are parameters defined for the VM name, password etc.
- every resource the VM needs is modelled, including the NIC and disk
- resources reference each other by name

Run a what-if deployment from the generated Bicep file to check that it is valid:

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/vm-simple-linux/azuredeploy.bicep --what-if --parameters adminUsername=linuxuser adminPasswordOrKey=<strong-password>
```

> You'll see output for all the resources to be created, and you may see warnings about the generated template.

📋 Fix the warnings in the Bicep file and update the NIC to use the static IP `10.1.0.103` - then deploy the template.

<details>
  <summary>Not sure how?</summary>

Here's an example of the updated file:

- [vm-simple-linux/azuredeploy-updated.bicep](/labs/arm-bicep/vm-simple-linux/azuredeploy-updated.bicep)

The sets the same _privateIP_ values we used in the ARM lab.

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/vm-simple-linux/azuredeploy-updated.bicep --parameters adminUsername=linuxuser adminPasswordOrKey=<strong-password>
```

</details><br/>

You should find the Bicep file much easier to navigate and edit.

## Evolving infrastructure specs

Bicep templates are typically meant to describe all the resources in the Resource Group. 

The default deployment mode for ARM is _incremental_ which means any new resources in the template get added, any matching ones are left as they are, and anything extra in the Resource Group (not described in the template) is left as-is.

- [vm-and-sql-db/main.bicep](/labs/arm-bicep/vm-and-sql-db/main.bicep) - adds a SQL Server and database spec to the existing Linux VM template

The resource identifiers have been tidied up from the generated Bicep, but the specs are the same. 

Run a what-if deployment and you'll see three new resources to be added and no changes to the existing resources:

```
az deployment group create -g labs-arm-bicep --template-file labs/arm-bicep/vm-and-sql-db/main.bicep  --parameters adminUsername=linuxuser adminPasswordOrKey=<strong-password> sqlAdminPassword=<strong-password> --what-if
```

## Lab

Bicep templates have very clear inputs and outputs. A common maintenance task is to move fixed settings in the template to parameters to make the deployment more flexible.

Update the Bicep template in `vm-and-sql-db/main.bicep` to make two settings configurable:

- the SQL Database SKU which must be one of `Basic` or `Standard`
- the name of a sample database schema to deploy, which must be one of `AdventureWorksLT` or `WideWorldImportersStd`

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Remember you need to delete the Resource Groups, not the deployment:

```
az group delete -y --no-wait -n labs-arm-bicep

az group delete -y --no-wait -n labs-arm-bicep-lab
```