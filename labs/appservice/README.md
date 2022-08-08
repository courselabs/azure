# App Service

## Reference

- [Azure App Service overview](https://docs.microsoft.com/en-us/azure/app-service/overview)

- [App Service Plan overview](https://docs.microsoft.com/en-us/azure/app-service/overview-hosting-plans)

- [`az appservice` commands](https://docs.microsoft.com/en-us/cli/azure/appservice?view=azure-cli-latest)

- [`az webapp` commands](https://docs.microsoft.com/en-us/cli/azure/webapp?view=azure-cli-latest)


## Explore App Service 

Create web app

- needs RG & app service plan
- publish: code, Docker, static
- runtime stack & OS

## Create an App Service Plan

az group create -n labs-appservice  -l westeurope --tags courselabs=azure

az appservice plan create -g labs-appservice -n app-service-01 --sku B1 --number-of-workers 2

> open RG in portal - only see app service plan; app list (empty), scale up and scale out options

Scale limited by SKU.

## Create an app for Git deployment

az webapp list-runtimes

az webapp create -g labs-appservice --plan app-service-01  --runtime 'ASPNET:V4.8' --deployment-local-git --name aspnet-app01 #<dns-unique-app-name>

> check again in portal

web app listed alongside app service; public URL is app name (https://aspnet-app01.azurewebsites.net); https provided

Browse to your app URL, you'll see "web app is running and waiting for your content"

## Deploy the web app

- set branch; expects `master` but we use `main`:
az webapp config appsettings set --settings DEPLOYMENT_BRANCH='main' -g labs-appservice -n aspnet-app01 #<dns-unique-app-name>

az webapp config appsettings set --settings PROJECT='src/WebForms/WebApp/WebApp.csproj' -g labs-appservice -n aspnet-app01 #<dns-unique-app-name>

- for info only - there is a remote git repo where you can push code & trigger deployment
az webapp deployment source config-local-git -g labs-appservice -n aspnet-app01 #<dns-unique-app-name>

> output id URL to use for git deployment

Get URL with default creds:

az webapp deployment list-publishing-credentials --query scmUri -g labs-appservice -n aspnet-app01 #<dns-unique-app-name> 

- creds embedded in URL (alternative is to create a dedicated user https://docs.microsoft.com/en-us/azure/app-service/deploy-configure-credentials?tabs=cli - but that has whole-subscription deployment permissions)

use single quotes as username contains dollar
git remote add webapp '<url-with-creds>'

verify
git remote -v



git push webapp main

- post deployment hook runs build, you'll see msbuild output

## Check the build

Portal - web app, browse

console blade

```
dir

dir .\bin
```

## Lab 

change homepage content and redeploy
