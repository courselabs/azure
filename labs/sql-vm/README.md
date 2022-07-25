# SQL Server VMs


## Reference

- [Azure SQL Server VM docs](https://docs.microsoft.com/en-us/azure/azure-sql/virtual-machines/?view=azuresql)

- [Database migration docs](https://docs.microsoft.com/en-us/azure/dms/tutorial-sql-server-to-managed-instance#create-an-azure-database-migration-service-instance)

- [`az sql mi` commands](https://docs.microsoft.com/en-us/cli/azure/sql/mi?view=azure-cli-latest)


## Explore Azure SQL in the Portal

Open the Portal and search to create a new Azure SQL resource. Check the details for _SQL Virtual Machines_:

* choose an image - there are Linux & Windows variants, different SQL Server versions and SKUs


We won't go on to create the database in the portal, we'll use the CLI instead.

## Create a Managed SQL Server with the CLI

First we need to create a Resource Group where the new SQL resources will live. This should be familiar from the [Resource Groups](/labs/resourcegroups/README.md) lab:

_Create the group - use your own preferred location:_

```
az group create -n labs-sql-vm --tags courselabs=azure -l westeurope
```

Next we need to find the VM image to use. We'll use SQL Server 2019 on a Windows Server 2022 machine:

```
# find the offers for SQL Server images - this includes Windows and Linux:
az vm image list-offers --publisher MicrosoftSQLServer

# list all the images, e.g:
az vm image list --offer sql2019-ws2022 --location westeurope -o table --all
```

📋 Create a SQL Server VM using the standard `vm create` command. 

<details>
  <summary>Not sure how?</summary>

This will get you started - be sure to use the latest image version, it will have a URN like this: _MicrosoftSQLServer:sql2019-ws2022:standard:15.0.220712_

```
az vm create -l westeurope -g labs-sql-vm -n sql01 --image <urn> --size Standard_D2_v3 --admin-username labs  --public-ip-address-dns-name  <your-dns-name> 
```

</details><br/>

If you open the VM in the Portal you'll see it's just a standard VM with no special SQL Server options.

> Check the Network Security Group. SQL Server will be listening on port 1433 - will you be able to access it from the Internet?

Even if you could access the VM, what is the admin username and password? You can't specify SQL Server authentication when you create a normal VM. To add to the management options, you need to register the VM with the [SQL Server IaaS extension](https://docs.microsoft.com/en-us/azure/azure-sql/virtual-machines/linux/sql-server-iaas-agent-extension-linux?view=azuresql&tabs=azure-powershell).

## Register the VM for SQL Server Management

TODO

📋 Register your VM for SQL Server management using `sql vm create` command. Configure it for public access and set a username and password for SQL Authentication

<details>
  <summary>Not sure how?</summary>

Print the help text:

```
az sql vm create --help
```

You need to specify:

- the VM name - this is the existing VM which is already running SQL Server
- license type - enterprises may have existing SQL Server licences to use
- management type - full gives you all the management options
- 

This will convert your VM to a SQL Server VM with public access:

```
az sql vm create -g labs-sql-vm -n sql01 --license-type PAYG --sql-mgmt-type Full --connectivity-type PUBLIC --sql-auth-update-username labs --sql-auth-update-pwd <strong-password> 00**abcDEF123!!$
```

</details><br/>

> Now browse to the VM in the Portal - the UI is exactly the same :) But open the Resource Group and you'll see there's a new SQL Virtual Machine resource.

From the Portal you can see your connectivity setup in the _Security Configuration_ blade. Check the NSG again and you'll see a new rule has been added to allow incoming traffic on port 1433.

## Customize the SQL Server VM

The SQL Server images have SSMS pre-installed, so you can log in and work with the database. First you'll need to enable RDP access for the VM. 

📋 Add an NSG rule to allow port 3389 connections from the Internet.

<details>
  <summary>Not sure how?</summary>

Find the name of your NSG:

```
az network nsg list -g labs-sql-vm  -o table
```

And add the RDP rule:

```
az network nsg rule create -g labs-sql-vm --nsg-name sql01NSG -n rdp --priority 150 --source-address-prefixes Internet --destination-port-ranges 3389 --access Allow
```

</details><br/>

Now you can log in to the VM:

- copy the DLL file  `labs/sql-vm/udf/FormattedDate.dll` from your machine to the VM
- run _SQL Server Management Studio_
- the default connection settings use the machine name and Windows auth, which is all fine
- connect then run this SQL to register the UDF

```
sp_configure 'show advanced options', 1
RECONFIGURE
GO

sp_configure 'clr enabled', 1
RECONFIGURE
GO

sp_configure 'clr strict security', 0
RECONFIGURE
GO

CREATE ASSEMBLY FormattedDate FROM 'C:\FormattedDate.dll';  
GO  
  
CREATE FUNCTION LegacyDate() RETURNS NVARCHAR(7)   
AS EXTERNAL NAME FormattedDate.FormattedDate.LegacyNow;   
GO  
  
SELECT dbo.LegacyDate();  
GO

```

You'll see the current date in a legacy system format. You can't do this with the other Azure SQL options because they don't give you access to the underlying OS.

## Lab

TODO

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

If you didn't finish the lab, you can delete the RG with this command to remove all the resources:

```
az group delete -y -n labs-sqlserver
```