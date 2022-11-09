# Continuous Deployment for Functions

`func` command for local dev and quick deployments, but need managed process for team delivery. Function apps can be configured for CI/CD from a Git repo, like other APp service apps.

## Reference

- [Functions Continuous Delivery with GitHub Actions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-how-to-github-actions?tabs=dotnet)

## Function App Deployment from GitHub

You should have your own fork of the lab repo in GitHub (we covered that in the [static web apps lab](/labs/appservice-static/README.md)). If not you can sign up for a free GitHub account and [create a fork](https://github.com/courselabs/azure/fork).

> If you already have a fork you can update it by clicking _Sync Fork_ in GitHub, or you can just delete it and create another one


📋 Create a Resource Group and a Function App using the consumption plan. Remember you'll need a Storage Account for the Function App.

<details>
  <summary>Not sure how?</summary>

Nothing much new here:

```
az group create -n labs-functions-cicd --tags courselabs=azure -l eastus

az storage account create -g labs-functions-cicd --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-cicd  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <fn-name>
```

</details><br/>


Function Apps can be deployed for GitHub Actions - the CLI will connect to your GitHub repo, ask for authentication and then create the pipeline:

```
az functionapp deployment github-actions add --branch main --build-path 'labs/functions/timer/TimerToBlob' --runtime-version 6 --login-with-github -g labs-functions-cicd --repo '<your-github-fork>' -n <fn-name> 
```


> Check the workflow file 

https://github.com/sixeyed/azure/tree/main/.github/workflows

And open the .YML file. The app name may not have been set correctly. If it looks like this:

```
env:
  AZURE_FUNCTIONAPP_NAME: 'your-app-name'   # set this to your function app name on Azure
```

You need to update it. Click on the edit icon (looks like a pen) and change the name to your actual function app name:

```
env:
  AZURE_FUNCTIONAPP_NAME: '<fn-name>'
```

Click opn the green _Start Commit_ button and then commit your changes.

## Configure the Function

Browse to the _Actions_ view and you'll see your app build and deploy.

Back in the Azure Portal, you will see the `Heartbeat` function listed (this is from the [Scheduled Functions lab](labs/functions/timer/README.md)):

- can you see the code that will run in the Portal?
- the bindings are listed in the Portal, can you edit the schedule?
- wait for the Function to run; why does it fail?

📋 Create the dependencies the Function needs, and configure the missing setting in the Function app.

<details>
  <summary>Not sure how?</summary>

The Function expects a separate storage account where it will write blobs. You need to create that and set the connection string in the app settings for the Function.

The bindings view for the Function tells you the configuration setting name that you need.

</details><br/>

Verify the next run of the function that it all works correctly and you see blobs created in your Storage Account.


## Add More Functions

There are some more functions which represent a chained workflow:

- labs/functions/cicd/update/WriteLog.cs
- labs/functions/cicd/update/NotifySubscribers.cs

We can add these to the main project and push the changes, which will trigger a build and deploy them:

```
# copy the new functions into the project folder:
cp labs/functions/cicd/update/*.cs labs/functions/cicd/ChainedFunctions/

# add a remote for your fork:
git remote add labs-funcions-cicd <your-github-fork-url>

# commit and push the changes:
git add labs/functions/cicd/ChainedFunctions/*
git commt -m 'New functions'
git push labs-funcions-cicd main
```

Watch the build progress in GitHub and then confirm the new functions are running. **One will fail**

📋 You need to create one more dependency in Azure, and set the configuration for one of the new functions.

<details>
  <summary>Not sure how?</summary>

The new Function writes a message to a Service Bus Queue - you need to create the namespace and the queue, then set the connection string as a Function App setting.

The bindings view for the Function tells you the configuration setting name that you need.

</details><br/>

## Lab

It's not very flexible having the trigger schedule and queue name hardcoded in the code files. Functions supports loading trigger and binding settings from configuration - using the syntax `%VariableName%` in place of the value you want to read from config. Upate `Heartbeat.cs` to use a variable for the schedule, and `NotifySubscribers.cs` to use a variable for the queue name. You can add those variables to a file called `local.settings.json` and test the functions locally with `func start`. When you have it working, push your changes and set the configuration in the Function App.

