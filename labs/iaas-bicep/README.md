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
az deployment group create -g labs-iaas-bicep --name sql --template-file labs/iaas-bicep/templates/sql.bicep --mode complete --what-if --parameters sqlAdminPassword=<sql-password>
```

> ARM gets confused here :) It says it would delete the VNet because it's not specified in the template, but then it would try to reference the subnet after deleting it...

We need to use incremental mode if we're splitting the deployment across multiple Bicep files:

```
az deployment group create -g labs-iaas-bicep --name sql --template-file labs/iaas-bicep/templates/sql.bicep --mode incremental --parameters sqlAdminPassword=<sql-password>
```

List out the resources again - you'll see the SQL Server name is a random string:

az resource list -g labs-iaas-bicep -o table

Check the Vnet rule is in place:

```
az sql server vnet-rule list -g labs-iaas-bicep --server <sql-server>
```

## VM

- vm.bicep

```
az deployment group create --what-if -g labs-iaas-bicep --name vm --template-file labs/iaas-bicep/templates/vm.bicep --mode incremental --parameters adminPassword=<vm-password> sqlServerName=<sql-server> sqlPassword=<sql-password>
```

```
az deployment group create -g labs-iaas-bicep --name vm --template-file labs/iaas-bicep/templates/vm.bicep --mode incremental --parameters adminPassword=<vm-password> sqlServerName=<sql-server> sqlPassword=<sql-password>
```

# check the VM:
az vm list -g labs-iaas-bicep -o table

# print the log from the setup script:
az vm run-command invoke  --command-id RunPowerShellScript -g labs-iaas-bicep --scripts "cat /vm-setup.log" -n <vm-name>


> Browse to the app at http://<vm-fqdn>/signup & verify it's working correctly

## Lab

Change the run command for vm & add rdp rule - what's happening in the logs?
