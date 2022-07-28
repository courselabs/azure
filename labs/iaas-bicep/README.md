# IaaS - Automating App Deployment

## Core resources

- core.bicep
- vars.json

```
az group create -n labs-iaas-bicep  -l westeurope --tags courselabs=azure

az deployment group create -g labs-iaas-bicep --name core --template-file labs/iaas-bicep/templates/core.bicep
```

Check the resources have been created as expected:

```
az resource list -g labs-iaas-bicep -o table
```

> You'll see the NSG and Vnet - not the subnet because that's a child resource of the VNet


## SQL Server

Incremental vs. complete mode - what-if if you choose the wrong one:

```
az deployment group create -g labs-iaas-bicep --name sql --template-file labs/iaas-bicep/templates/sql.bicep --mode complete --what-if --parameters sqlAdminPassword='sadfu324**' #<strong-password>
```

> ARM gets confused here :) It says it would delete the VNet because it's not specified in the template, but then it would try to reference the subnet after deleting it...

We need to use incremental mode if we're splitting the deployment across multiple Bicep files:

```
az deployment group create -g labs-iaas-bicep --name sql --template-file labs/iaas-bicep/templates/sql.bicep --mode incremental --parameters sqlAdminPassword='sadfu324**' #<strong-password>
```

List out the resources again - you'll see the SQL Server name is a random string:

az resource list -g labs-iaas-bicep -o table

Check the Vnet rule is in place:

```
az sql server vnet-rule list -g labs-iaas-bicep --server
```

## VM

- vm.bicep

```
az deployment group create --what-if -g labs-iaas-bicep --name vm --template-file labs/iaas-bicep/templates/vm.bicep --mode incremental --parameters adminPassword='sadfu324**' #<strong-password>
```

```
az deployment group create -g labs-iaas-bicep --name vm --template-file labs/iaas-bicep/templates/vm.bicep --mode incremental --parameters adminPassword='sadfu324**' #<strong-password>
```


## Lab

UPdate the SQL Bicep to include size options & re-deploy for smaller capacity; re-deploy VM with bigger size
