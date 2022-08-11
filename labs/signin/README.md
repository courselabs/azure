# Azure Signin

Azure uses Microsoft accounts to authenticate and authorize. In a corporate environment your account would be managed for you, but you can create your own personal account and create an Azure Subscription for your own use. One account can have permissions on multiple Subscriptions.

## Reference

- [Subscriptions](https://docs.microsoft.com/en-gb/learn/modules/configure-subscriptions/3-implement-azure-subscriptions)

- [Azure documentation home](https://docs.microsoft.com/en-gb/azure/?product=popular)

- [az Command Line Interface](https://docs.microsoft.com/en-us/cli/azure/reference-index?view=azure-cli-latest)


## Exploring the Azure Portal

Browse to https://portal.azure.com and sign in with your Microsoft account.

Open the [All Services](https://portal.azure.com/#allservices) view. This is where you can see everything in Azure. There is a lot...

- Find the _Subscriptions_ service and open it. This is where you see all the Subscriptions you have access to.

- Open the _Virtual Machines_ view. How would you create a new Windows VM, and what sort of configuration settings do you need to specify?

- Browse back to Azure Home and find the _Quickstart Center_. What do you see in the reference architecture for Azure Web Apps?

> The Portal is a great way to browse Azure services and explore resources. But it doesn't give you a repeatable, automatable experience. 


## Using the Azure CLI

The [az](https://docs.microsoft.com/en-us/cli/azure/) command line is another option for interacting with Azure. It's the tool I recommend because:

- it's one of the first to be updated with new features and services
- it has integrated help
- you can use it for creating and managing resources
- it's cross-platform and easy to script in CI/CD pipelines

Try running the `az` command to see how the integrated help works.

Upgrade to make sure you have the latest version:

```
az upgrade
```

Then login to Azure:

```
az login
```

📋 Use the command line to list all the accounts you have access to. Experiment with different outputs to find the most user-friendly.

<details>
  <summary>Not sure how?</summary>

This shows your account and Subscriptions:

```
az account list
```

And use the `-o` or `--output` flag to change between JSON, YAML and table formats:

```
az account list -o table
```

</details><br/>

> You'll see a unique ID for each Subscription. What do you think the `CloudName` field represents?

## The Azure Shell

Sometimes you want to use the Azure Shell but you can't install the CLI. Then you can use the Azure Cloud Shell, which is a web interface that gives you a terminal session with Azure tools installed and configured.

Browse to https://shell.azure.com

- you can choose between PowerShell or Bash for your terminal
- the shell session uses Azure storage to persist files and command history
- the `az` command line is already installed

Try running:

```
az account list -o tsv
```

> You'll see your subscriptions listed in tab-separated-variable format. You're already signed in because you authenticated with the Portal.

It's harder to explore the Cloud Shell with the command line, but if you search you'll see it has lots of tools pre-installed. 

## Lab

This is a quick lab to show you what you can do with the Cloud Shell. There's a C# source file in this folder:

- [labs/signin/src/Program.cs](./src/Program.cs)

Use the `dotnet new` command in the Cloud Shell to create a new console project. Upload the C# file to the project directory and run it.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).
