# IaaS - Automating App Deployment

The IaaS approach doesn't mean you have to manually log in to VMs and deploy applications - you can still take advantage of automation. You can model out your infrastructure using ARM templates or Bicep, and your VM setup can include delpoyment scripts. Those scripts can automatically install application dependencies, the application itself and set up configuration files.

In this lab we'll use Bicep to define a deployment which includes a Windows VM and a SQL Server database, and deploys a .NET application to the VM.

## Core resources

Bicep supports larger infrastructure requirements by letting you split the model across multiple files. You can share variable names between files, which lets you refer to resources defined in different Bicep files:

- [templates/vars.json](/labs/iaas-bicep/templates/vars.json) - just defines variables, which are the names of resources that get referenced in the Bicep files; this is plain JSON

- [templates/core.bicep](/labs/iaas-bicep/templates/core.bicep) - references the variables file with the `loadJsonContent` function; it defines the core resources - a virtual network, subnet and NSG.

It's worth taking some time to look at this Bicep file. It's small enough to be fairly easy to understand, but there are some useful features in use:

- there's a parameter for the location to deploy to, but it's optional - where does the default value come from?

- all the resource names are defined outside of this file - where are they set and how does this file refer to them?

- objects which have a parent/child relationship aren't defined as nested, so the file is much easier to read - how are relationships specified?

📋 Create a Resource Group called `labs-iaas-bicep` and deploy the Core Bicep file (we covered this in the [Bicep lab](/labs/arm-bicep/README.md)).

<details>
  <summary>Not sure how?</summary>

This is straightforward - a `group create` and a `deployment group create`:

```
az group create -n labs-iaas-bicep --tags courselabs=azure -l westeurope 

az deployment group create -g labs-iaas-bicep --name core --template-file labs/iaas-bicep/templates/core.bicep
```

</details><br/>

Check the resources have been created as expected:

```
az resource list -g labs-iaas-bicep -o table
```

> You'll see the NSG and virtual network - not the subnet because that's a child resource of the VNet

## SQL Server

We have a second Bicep file here which defines the SQL Server database resources:

- [templates/sql.bicep](/labs/iaas-bicep/templates/sql.bicep) - defines a SQL Server and a database, loading the shared variable JSON file

There's some good stuff in this template too:

- most of the parameters have default values, how does the template make sure the SQL Server name is a DNS name which isn't already in use?

- the admin password is required, but it has special treatment - what does the `secure` flag do?

- the SQL Server is created in the subnet defined in the Core Bicep file - how does this file reference a resource which is not defined here?

ARM deployments can run in one of two modes:

- _complete_ - where the template you're deploying contains the full definition of all your resources

- _incremental_ - where the template defines part of your model, and is expected to add to other resources

The default mode is _complete_ so if we try to deploy the SQL Bicep file we'll have issues.

Use the `what-if` flag to see what would happen in the deployment:

```
az deployment group create -g labs-iaas-bicep --name sql --template-file labs/iaas-bicep/templates/sql.bicep --mode complete --what-if --parameters sqlAdminPassword=<sql-password>
```

> ARM gets confused here :) It says it would delete the VNet because it's not specified in the template, but then it would try to reference the subnet after deleting it...

📋 Deploy the SQL Bicep file as an incremental deployment to the same RG as the Core Bicep resources.

<details>
  <summary>Not sure how?</summary>

We need to use incremental mode if we're splitting the deployment across multiple Bicep files:

```
az deployment group create -g labs-iaas-bicep --name sql --template-file labs/iaas-bicep/templates/sql.bicep --mode incremental --parameters sqlAdminPassword=<sql-password>
```

</details><br/>

List out the resources again - you'll see none of the Core resources have been removed, the news ones are added and the SQL Server name is a random string:

```
az resource list -g labs-iaas-bicep -o table
```

One of the child resources of the SQL Server is a VNet rule - this is a firewall setup which allows access to SQL Server for any resource in the same virtual network. Check the rule is in place: 

```
az sql server vnet-rule list -g labs-iaas-bicep --server <sql-server>
```

## Windows Application VM

The final Bicep file for this application defines the Windows Server VM. The application we'll be deploying is the same one as the [IaaS apps lab](/labs/iaas-apps/README.md), but all the steps are automated here:

- [templates/vm.bicep](/labs/iaas-bicep/templates/vm.bicep) - includes the VM and the resources it needs - the NIC and PIP, with references to the Core resources via the shared JSON variables file

There's some more to unpack from this template:

- object names are derived from other object names - but the SQL Server name is repeated from the SQL Bicep file, what are the advantages and disadvantages of that?

- the Bicep resources define the IaaS components, but there's an additional resource to run a PowerShell script after the VM is created; that will run the steps in [scripts/vm-setup.ps1](/labs/iaas-bicep/scripts/vm-setup.ps1) to deploy the application.

📋 Deploy the VM Bicep file as an incremental deployment to the RG - making sure it won't do anything unexpected.

<details>
  <summary>Not sure how?</summary>

Run the what-if deployment:

```
az deployment group create --what-if -g labs-iaas-bicep --name vm --template-file labs/iaas-bicep/templates/vm.bicep --mode incremental --parameters adminPassword=<vm-password> sqlPassword=<sql-password>
```

And if it all looks good, go on to deploy: 

```
az deployment group create -g labs-iaas-bicep --name vm --template-file labs/iaas-bicep/templates/vm.bicep --mode incremental --parameters adminPassword=<vm-password> sqlPassword=<sql-password>
```

</details><br/>

Check that the VM has been created:

```
az vm list -g labs-iaas-bicep -o table
```

The setup script writes log entries to a file; you can use another run-command to print the output of the log file:

```
az vm run-command invoke  --command-id RunPowerShellScript -g labs-iaas-bicep --scripts "cat /vm-setup.log" -n <vm-name>
```

> Browse to the app at `http://<vm-fqdn>/signup` & verify it's working correctly

## Lab

There are a couple of issues with the VM Bicep file. The first is the warning you get when you deploy - it's not really an issue for us, but we should follow the best practice. Also we had to query the VM and manually build the URL to test. Update the Bicep file to address both of those issues then deploy it again. Does the setup script get run again?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG:

```
az group delete -y -n labs-iaas-bicep
```