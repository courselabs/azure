# IaaS - Deploying Applications

## Create Resource Group

```
# set some variables - PowerShell:
$location='westeurope'
$rg='labs-iaas-apps'
$server='labs-iaas-apps-es' # <unique-server-name>
$database='signup'

# OR Bash:



# create RG
az group create -n $rg  -l $location --tags courselabs=azure
```

## Create SQL Database

- new server
- new database

```
az sql server create -g $rg -l $location -n $server -u sqladmin -p 'wesfdDSDH232***' #<admin-password>

az sql db create -g $rg -n $database -s $server --no-wait
```


## Create Windows Server VM

- create windows server 2022 
- connect & install .net 4.8

az vm image list-skus -l westus -p MicrosoftWindowsServer -f WindowsServer -o table

> Look for a 2022 datacenter core SKU

az vm create -l $location -g $rg -n app01 --image MicrosoftWindowsServer:WindowsServer:2022-datacenter-core-g2:latest --size Standard_D2s_v5 --admin-username labs --public-ip-address-dns-name <your-unique-dns-name> --admin-password <your-strong-password>

## Deploy the app

RDP into the VM using the DNS name and your credentials

> This is a Windows Server Core VM so there's no UI - you'll drop into a PowerShell session


Check the .NET version:

```
Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP' -recurse |
Get-ItemProperty -name Version,Release -EA 0 |
Where { $_.PSChildName -match '^(?!S)\p{L}'} |
Select PSChildName, Version, Release
```

We need at least 4.8.

Install IIS:

```
Get-WindowsFeature

# IIS and ASP.NET not installed by default

Install-WindowsFeature Web-Server,NET-Framework-45-ASPNET,Web-Asp-Net45

```



- download & run msi

curl -o signup.msi https://github.com/courselabs/azure/releases/download/labs-iaas-apps-1.0/SignUp-1.0.msi

Start-Process msiexec.exe -ArgumentList '/i', 'signup.msi', '/quiet', '/norestart' -NoNewWindow -Wait


- edit connection strings
- test locally with curl

Check the installation:

```
ls /docker4.net

get-webapplication

curl.exe http://localhost/signup
```

> This will take a while to respond and then show an error - _The server was not found or was not accessible._ 

The website is using a default configuration file - we need to edit that to use our SQL Azure database.


## Fix connectivity

The default config file needs to be updated to use the correct database connection string:

```
cat C:\docker4.net\SignUp.Web\connectionStrings.config
```

We need to replace the connection string value `"Server=SIGNUP-DB-DEV01;Database=SignUp;User Id=sa;Password=DockerCon!!!;Connect Timeout=10;"` with the correct server name and credentials.

Find the connection string for your database (the Portal is good for this) and update the config file:

```
notepad  C:\docker4.net\SignUp.Web\connectionStrings.config
```

Try the app again locally with curl - you'll get a new error :) The SQL database needs to be configured to allow access from the VM.

Open the SQL Server (the server not the database) in the Portal and select the _Networking_ tab.

- add a virtual network rule to allow access from the vnet the VM is connected to
- pay attention to the messages from the UI...

Test the app again with curl in the VM - you should see an HTML response with no errors

## Lab

Now the app is working locally, we need to publish it so we can access it from the Internet. Make the changes you need so you can browse to the VM's DNS name from your machines and see the application:

- http://[vm-fqdn]/signup

![/img/signup-homepage.png]

Click the _Sign Up_ button and add some details. Run some queries in the SQL database to verify your data is saved.



