# App Service for Web Apps

IaaS options are great when you need access to the host machine to configure and deploy your app, but it leaves you with a lot of management overhead. Platform-as-a-Service (PaaS) takes care of that for you, simplifying deployment and updates, provided your application is supported in the PaaS environment. Azure has a few PaaS options - App Service is a very popular one.

In this lab you'll create an App Service deployment by pushing source code from your local machine. Azure will compile and configure the app for you.

## Reference

- [Azure App Service overview](https://docs.microsoft.com/en-us/azure/app-service/overview)

- [App Service Plan overview](https://docs.microsoft.com/en-us/azure/app-service/overview-hosting-plans)

- [`az appservice` commands](https://docs.microsoft.com/en-us/cli/azure/appservice?view=azure-cli-latest)

- [`az webapp` commands](https://docs.microsoft.com/en-us/cli/azure/webapp?view=azure-cli-latest)


## Explore App Service 

Create a new resource in the Portal - search for _Web app_ (which is one of the App Service types):

- the app needs a Resource Group and an App Service Plan
- you have options to publish from: source code, Docker containers, static web content
- for the source code option, you can choosse the runtime stack & OS (e.g. Java on Linux or .NET on Windows)

As usual, we won't create from the Portal, we'll switch to the CLI.

## Create an App Service Plan

Create a Resource Group for the lab:

```
az group create -n labs-appservice  -l westeurope --tags courselabs=azure
```

Before we can create the app we need an App Service Plan - which is an abstraction of the infrastructure needed to run applications.

📋 Create an App Service Plan using the basic B1 SKU, and with two instances.

<details>
  <summary>Not sure how?</summary>

This is fairly straightforward: 

```
az appservice plan create -g labs-appservice -n app-service-01 --sku B1 --number-of-workers 2
```

</details><br/>

Open the RG in the Portal. The only resource is the App Service Plan. Open that and you'll see an empty app list, and the scale up and scale out options (which are limited by the plan SKU).

## Create an app for Git deployment

We can create a Web App using the new App Service Plan. List the available runtimes to see what platforms are supported:

```
az webapp list-runtimes
```

Under the Windows options we have ASP.NET 4.8. This would work for pretty much any older .NET applications, and is a good fit for migrating apps to the cloud if you have the source code and you don't need the control you get with IaaS.

📋 Create web app in the service plan using the ASP.NET 4.8 runtime, and set for deployment from a local Git repository.

<details>
  <summary>Not sure how?</summary>

Check the help text for a new web app:

```
az webapp create --help
```

You need to specify the runtime, deployment method and a unique DNS name for the app:

```
az webapp create -g labs-appservice --plan app-service-01  --runtime 'ASPNET:V4.8' --deployment-local-git --name <dns-unique-app-name>
```

</details><br/>

Check the RG again in the Portal when your CLI command has completed.

> Now the web app is listed as a separate resource - the type is _App Service_ - but you can navigate to the plan from the app and vice versa

Open the web app and you'll see it has a public URL, which uses the application name you set; HTTPS is provided by the platform. 

Browse to your app URL, you'll see a landing page saying "Your web app is running and waiting for your content".

## Deploy the web app

Deploying to the web app from a local Git repository is as easy as running `git push` - but we need to configure a few things first. The source code is in this repository, but we need to tell Azure which branch to use and the path to the code.

You can apply configuration settings for web apps which your application can read - and they can also be used by the platform. These settings will make sure the correct application code gets deployed:

```
# we're using the main branch:
az webapp config appsettings set --settings DEPLOYMENT_BRANCH='main' -g labs-appservice -n <dns-unique-app-name>

# and the code is in the WebForms folder:
az webapp config appsettings set --settings PROJECT='src/WebForms/WebApp/WebApp.csproj' -g labs-appservice -n <dns-unique-app-name>
```

The way this works is that the web app acts as a Git server. You can set the web app as a remote repository and push your code. Whenever the code is pushed it gets compiled and the web app is configured to run it.

📋 Print the publishing credentials for your web app deployment - specifically the `scmUri` - which is the remote Git location you'll need to use.

<details>
  <summary>Not sure how?</summary>

There are a lot of subcommands for the web app. Listing the publishing credentials gives you the Git URL and credentials:

```
az webapp deployment list-publishing-credentials --query scmUri -g labs-appservice -o tsv -n <dns-unique-app-name> 
```

</details><br/>

> The Git credentials are embedded in the URL, which is a security nightmare. There are [alternative options](https://docs.microsoft.com/en-us/azure/app-service/deploy-configure-credentials?tabs=cli) but this will do for the lab

You can add the web app SCM as a remote to your Git repo, with the output from the credentials command:

```
# use single quotes as username contains a dollar sign:
git remote add webapp '<url-with-credentials>'

# verify that the remote is saved correctly:
git remote -v
```

You can deploy the app now by pushing your local repo up to the webapp remote - be sure to use the correct branch:

```
git push webapp main
```

You'll see the usual Git output about compressing and writing objects, but then you'll see a lot more. The remote generates a deployment script and you'll see MSBuild output, which is the .NET application being compiled. When the `git push` is complete, the app has been compiled and deployed.

## Check the build

Refresh the URL for your web app and you'll see a standard ASP.NET homepage. It's not much of an application, but we deployed it from source code with no VMs or build servers or anything else in just a few minutes.

There's no VM where you can connect to diagnose issues, but the Portal gives you lots of tools to help. Open the web app blade and navigate to the _Console_ option - this connects you to a terminal session in the web app host.

Explore the filesystem - these folders were created and populated by the deployment script:

```
dir

dir bin
```

List the environment variables in the host and you'll see lots of App Service-specific settings:

```
set
```

## Lab 

The web app is a bit boring. Change the content of the home page and redeploy it. How long does it take for the update to happen?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG to clean up:

```
az group delete -y -n labs-appservice
```
