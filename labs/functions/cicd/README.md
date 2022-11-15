# Continuous Deployment for Functions

The `func` command line is great for local development and quick prototypes, but you need managed process based on centralized SCM for team delivery. Function apps can be configured for CI/CD from a Git repo like other App Service apps.

In this lab we'll deploy a set of chained functions, using CI/CD with GitHub Actions, and test the workflow with some function updates.

## Reference

- [Functions Continuous Delivery with GitHub Actions](https://learn.microsoft.com/en-us/azure/azure-functions/functions-how-to-github-actions?tabs=dotnet)

## Function App Deployment from GitHub

You should have your own fork of the lab repo in GitHub (we covered that in the [static web apps lab](/labs/appservice-static/README.md)). If not you can sign up for a free GitHub account and [create a fork](https://github.com/courselabs/azure/fork).

> If you already have a fork you can update it by clicking _Sync Fork_ in GitHub, or you can just delete it and create another one

We'll get started with Azure straight away. 

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


Function Apps can be deployed with GitHub Actions. The Azure CLI can connect to your GitHub repo, ask for authentication and then create the pipeline YAML file.

Create a Function App with GitHub deployment - this will open a browser asking for a code which is displayed back in the terminal:

```
az functionapp deployment github-actions add --branch main --build-path 'labs/functions/cicd/ChainedFunctions' --runtime-version 6 --login-with-github -g labs-functions-cicd --repo '<your-github-fork>' -n <fn-name> 
```

> When that completes, check the workflow file in your GitHub fork

GitHub Actions workflows are stored in YAML files in the `.github` folder in your repo. Browse to the file in GitHub and check the contents. **The Function App name may not have been set correctly**. 

If it looks like this:

```
env:
  AZURE_FUNCTIONAPP_NAME: 'your-app-name'   # set this to your function app name on Azure
```

You need to update it. Click on the edit icon in GitHub (which looks like a pen) and change the name to your actual function app name:

```
env:
  AZURE_FUNCTIONAPP_NAME: '<fn-name>'
```

Click opn the green _Start Commit_ button and then commit your changes. That will trigger a new build.

## Configure the Function

Browse to the _Actions_ view and you'll see your app build and deploy.

Back in the Azure Portal, you will see the `Heartbeat` function listed (the code is similar to the [Scheduled Functions lab](/labs/functions/timer/README.md)):

- can you see the code that will run in the Portal?
- the bindings are listed in the Portal, can you edit the schedule?
- wait for the Function to run; why does it fail?

📋 Create the dependencies the Function needs, and configure the missing setting in the Function app.

<details>
  <summary>Not sure how?</summary>

The Function expects a separate storage account where it will write blobs. You need to create that and set the connection string in the app setting for the Function.

The bindings view for the Function tells you the configuration setting name that you need.

</details><br/>

Verify with the next run of the function that it all works correctly and you see a blob created in your Storage Account.

## Add More Functions

There are some more functions we can add to the project, which represent a chained workflow - where the output of one function acts as the trigger for the next:

- [ChainedFunctions/Heartbeat.cs](/labs/functions/cicd/ChainedFunctions/Heartbeat.cs) - the originating function is triggered by a timer and writes to blob storage
- [update/WriteLog.cs](/labs/functions/cicd/update/WriteLog.cs) - triggered by the blob creation, writes an entity in Table Storage
- [update/NotifySubscribers.cs](/labs/functions/cicd/update/NotifySubscribers.cs) - also triggered by the blob creation, publishes a message to Azure Service Bus

We can add these to the main project and push the changes, which will trigger a build and deploy them:

```
# add a remote for your fork:
git remote add labs-funcions-cicd <your-github-fork-url>

# pull changes to download the workflow:
git pull labs-funcions-cicd

# copy the new functions into the project folder:
cp labs/functions/cicd/update/*.cs labs/functions/cicd/ChainedFunctions/

# commit and push the changes:
git add labs/functions/cicd/ChainedFunctions/*
git commit -m 'New functions'
git push labs-funcions-cicd main
```

Watch the build progress in GitHub and then confirm the new functions are running. **One will fail**

📋 You need to create one more dependency in Azure, and set the configuration for one of the new functions.

<details>
  <summary>Not sure how?</summary>

The new Function writes a message to a Service Bus Queue - you need to create the namespace and the queue, then set the connection string as a Function App setting.

The bindings view for the Function tells you the configuration setting name that you need.

</details><br/>

When you get it all running, you can see how the timer trigger ultimately results in three outputs - the blob, the Table Storage entity and the Service Bus message.

## Lab

There are some questions around the chaining of functions here. Why doesn't the Service Bus notification get triggered from the Table Storage write? Then the functions would be guaranteed to run in a known sequence. And if the real goal is the Table Storage entity and the message, can we restructure this to avoid creating a blob?


> Stuck? Try [suggestions](suggestions.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-cicd
```
