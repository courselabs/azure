# SQL Azure

Azure offers multiple services for SQL Server - from serverless options where you only pay when the database is in use, to managed VMs which have complete feature parity with SQL Server in the datacenter. You'll need to know all the options, but typically one will fit for most of your workloads.

## Reference

- [Azure SQL docs](https://docs.microsoft.com/en-gb/azure/azure-sql/)

- [`az sql server` commands](https://docs.microsoft.com/en-us/cli/azure/sql/server?view=azure-cli-latest)

- [`az sql db` commands](https://docs.microsoft.com/en-us/cli/azure/sql/db?view=azure-cli-latest)


## Explore Azure SQL in the Portal

Open the Portal and search to create a new Azure SQL resource. There are different types of service:

- which would you choose for a new app which has occasional SQL usage? 
- why might you need the Virtual Machine option?

Select the _SQL databases_ otion and choose to create a _Single database_. Look at the required options - what other resources do you need to create before you get to the database?

> A SQL database belongs to a SQL Server instance, which belongs in a resource group; you can typically create dependent resources directly in the portal.

Follow the link to create a new SQL Server for your database:

- you need a server name and a location. Can you use any name?
- you also need to select the authentication types. Windows authentication is preferred in the datacenter, but the default here is for SQL authentication. Why might that be more suitable in the cloud?

We won't go on to create the database in the portal, we'll use the CLI instead.

## Create a SQL Server with the CLI

First we need to create a Resource Group where the new SQL resources will live. This should be familiar from the [Resource Groups](/labs/resourcegroups/README.md) lab:

_Create the group - use your own preferred location:_

```
az group create -n labs-sql --tags courselabs=azure -l westeurope
```

Now you can create the SQL Server which will be the host for the database.

> You'll need to find a globally unique name for the server, because it gets used as the public DNS name.

📋 Create a database server using a `sql server create` command. There are a few parameters you'll need to specify.

<details>
  <summary>Not sure how?</summary>

Print the help text:

```
az sql server create --help
```

As a minimum you need to specify:

- resource group
- location
- server name (must be globally unique)
- administrator account name
- administrator password (must meet the password policy) 

This will get you started:

```
# you'll need to supply your own name and password:
az sql server create -l eastus -g labs-sql -n <server-name> -u sqladmin -p <admin-password>
```

</details><br/>

> Creating a new SQL Server takes a few minutes. While it's running, check the docs to answer this:

- what is the running cost for a SQL Server with no databases?

When your SQL Server is created, browse to the portal and find the server properties. Now you can see that the server name needs to be globally unique.

## Create a SQL Database

The SQL Server is a container for zero or more databases. When it's created you can use the `sql db create` command to create a new database in the server.

📋 Create a database called `db01` in your SQL Server using the `az` command.

<details>
  <summary>Not sure how?</summary>

You need to supply the SQL Server name, resource group and a database name:

```
az sql db create -g labs-sql -n db01 -s <server-name>
```

</details><br/>

> This will also take a couple of minutes; check the portal to see the status. In the meantime, can you answer:

- what is the default size for a new database?
- why don't you need to supply admin credentials for the new database?

When the database is created, it's just a standard SQL Server instance which you can connect to from a remote client.

## Connect to the Database

The portal view for SQL Databases shows connection strings. Use that to connect to the database with a SQL client:

- you can use Visual Studio or SQL Server Management Studio if you have them
- or the [SQL Server Extension for VS Code](https://docs.microsoft.com/en-us/sql/tools/visual-studio-code/sql-server-develop-use-vscode?view=sql-server-ver15)
- or a simple client like [Sqlectron](https://github.com/sqlectron/sqlectron-gui/releases/tag/v1.32.1) (don't download a newer version than 1.32 because of [issue 699](https://github.com/sqlectron/sqlectron-gui/issues/699))

📋 Try to connect with the SQL Server credentials - can you access the database?

<details>
  <summary>Not sure?</summary>

You'll see an error like this:

*Cannot open server 'sql-labs-03' requested by the login. Client with IP address '216.213.184.119' is not allowed to access the server. To enable access, use the Windows Azure Management Portal or run sp_set_firewall_rule on the master database to create a firewall rule for this IP address or address range. It may take up to five minutes for this change to take effect.*

</details><br/>

SQL Server has an IP block, so you need to explicitly allow access to clients based on the originating IP address.

In the portal, open the **SQL Server** instance (not the database) and find the firewall settings. On that page you can easily add your own IP address to the rules, so you will be allowed access - then try the connection again.

## Query the Database

When you successfuly connect, you're using the administrator credentials so you can run DDL and DML statements:

```
CREATE TABLE students (id INT IDENTITY, email NVARCHAR(150))

INSERT INTO students(email) VALUES ('elton@sixeyed.com')

SELECT * FROM students
```

> You could use this database with a .NET application - setting the connection string in config, and having the database schema automatically created with Entity Framework.

## Lab

Use CLI to delete the SQL database. When the database is gone the SQL Server still exists - can you retrieve the data from your deleted database? Now delete the resource group, does the SQL Server still exist?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

If you didn't finish the lab, you can delete the RG with this command to remove all the resources:

```
az group delete -y -n labs-sql
```