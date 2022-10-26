# IaaS - Deploying Applications

Infrastructure-as-a-Service is an easy way to get started with cloud deployments or cloud migrations. You can configure VMs with whatever they need and deploy any type of application. You might have a roadmap to move your apps to Platform-as-a-Service (PaaS), but IaaS can be a good starting point. 

In this lab you'll create the infrastructure and use it to deploy an old .NET app on Windows, which uses a SQL Server database.


## Create Resource Group

We'll be creating a few resources, so start by storing some variables we can reuse:

```
# inPowerShell:
$location='westeurope'
$rg='labs-iaas-apps'
$server='<unique-dns-name>'
$database='signup'

# OR in Bash:
location='westeurope'
rg='labs-iaas-apps'
server='<unique-dns-name>'
database='signup'

# create the RG:
az group create -n $rg  -l $location --tags courselabs=azure
```

## Create SQL Database

The application we're going to deploy will create the database schema, so we just need to start with an empty database we can connect to.

📋 Create a SQL Server and a SQL Database in the Resource Group (we covered this in the [SQL lab](/labs/sql/README.md)).

<details>
  <summary>Not sure how?</summary>

We can use simple commands with the variables we have already set, and accept default values for a lot of the configuration settings:

```
az sql server create -g $rg -l $location -n $server -u sqladmin -p '<admin-password>'

az sql db create -g $rg -n $database -s $server --no-wait
```

</details><br/>

We don't need to wait for the SQL database to be ready, we can move on to the next stage.

## Create Windows Server VM

Our application needs the .NET Framework to run. That's an older Windows-only platform, but it's still supported in the latest Windows Server releases.

Windows Server VM images are listed under the publisher `MicrosoftWindowsServer` and the offer `WindowsServer`.

📋 Create a VM using the latest release of Windows Server 2022, with the Datacenter Core 2nd-generation SKU (we saw how to search SKUs in the [Windows VM lab](/labs/vm-win/README.md)).

<details>
  <summary>Not sure how?</summary>

```
# list out the SKUs:
az vm image list-skus -l westus -p MicrosoftWindowsServer -f WindowsServer -o table
```

Among the outputs you should see `2022-datacenter-core-g2` which is the SKU we're looking for.

```
# create a VM using the latest version of the image:
az vm create -l $location -g $rg -n app01 --image MicrosoftWindowsServer:WindowsServer:2022-datacenter-core-g2:latest --size Standard_D2s_v5 --admin-username labs --public-ip-address-dns-name <your-unique-dns-name> --admin-password <your-strong-password>
```

</details><br/>

You don't need to add a DNS name for the PIP, but if you do it will be easier to connect to the VM.

## Deploy the app

When the VM is up and running, we can connect and deploy the app. We'll do that manually in this lab, and we'll look at automation options later.

Use a Remote Desktop client to connect to the VM using the public IP address or DNS name, and the credentials you set when you created it. 

> This is a Windows Server Core VM so there's no familiar Windows GUI - you'll drop into a terminal session.

Now we can follow the app deployment instructions, which follow a common pattern:

- install dependencies to run the app
- install the application
- configure application settings

### .NET Framework 4.8

The VM will have .NET already installed - start by checking the version:

```
# this prints the .NET Framework version(s) installed:
Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP' -recurse |
Get-ItemProperty -name Version,Release -EA 0 |
Where { $_.PSChildName -match '^(?!S)\p{L}'} |
Select PSChildName, Version, Release
```

We need the _Full_ framework to be _4.8_ (if not we would need to install it).

### Web Server

The application needs a web server. Windows Server runs Internet Information Services (IIS), but it's not installed by default. Install it by adding the Windows features:

```
# verify the IIS components are not installed:
Get-WindowsFeature

# then install all the bits we need:
Install-WindowsFeature Web-Server,NET-Framework-45-ASPNET,Web-Asp-Net45
```

### Application install

The application is packaged as a Windows Installer MSI, which is published on GitHub. 

Download and run the MSI to install the app:

```
# download the package:
curl -o signup.msi https://github.com/courselabs/azure/releases/download/labs-iaas-apps-1.0/SignUp-1.0.msi

# deploy it:
Start-Process msiexec.exe -ArgumentList '/i', 'signup.msi', '/quiet', '/norestart' -NoNewWindow -Wait
```

When the process completes, check that the application has been deployed:

```
# list the files created by the package:
ls /docker4.net/SignUp.Web

# verify that the application has been registered with the web server:
Get-WebApplication
```

You can test the application on the VM by making an HTTP request to localhost. You'll get a response here but it will be full of error logs - we haven't finished the deployment yet:

```
curl.exe -L http://localhost/signup
```

> This will take a while to respond and then show an error - _The server was not found or was not accessible._ 

The website is using a default configuration file. We need to edit that to use our SQL Azure database.


### Application configuration

The default config file needs to be updated to use the correct database connection string:

```
# print the default database connection details:
cat C:\docker4.net\SignUp.Web\connectionStrings.config
```

We need to replace the connection string value `"Server=SIGNUP-DB-DEV01;Database=SignUp;User Id=sa;Password=DockerCon!!!;Connect Timeout=10;"` with the correct server name and credentials.

Find the connection string for your database (the Portal is good for this) and update the config file:

```
# Windows Server Core doesn't have the full GUI but it does have Notepad :)
notepad C:\docker4.net\SignUp.Web\connectionStrings.config
```

Try the app again locally with curl - you'll get a new error :)

```
curl.exe -L http://localhost/signup
```

The SQL database needs to be configured to allow access from the VM.

### Database configuration

Open the SQL Server (the server not the database) in the Portal and select the _Networking_ tab.

- add a virtual network rule to allow access from the vnet the VM is connected to
- pay attention to the messages from the UI...

Test the app again with curl in the VM - you should see an HTML response with no errors:

```
curl.exe -L http://localhost/signup
```

## Lab

Now the app is working locally, we need to publish it so we can access it from the Internet. Make the changes you need so you can browse to a DNS name from your local machine and see the application:

- http://[vm-fqdn]/signup

![Sign Up app](/img/signup-homepage.png)

Click the _Sign Up_ button and add some details. Run some queries in the SQL database to verify your data is saved.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG:

```
az group delete -y -n $rg
```