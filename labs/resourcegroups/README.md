# Resource Groups

Resource Groups (RGs) are a container for all other Azure resources - VMs, SQL databases, Kubernetes clusters all get created inside a Resource Group. You might have one Resource Group for each application, containing all the components that app needs. Management permissions can be applied at the Resource Group level, and it's easy to remove all the resources by deleting the group.

## Reference

- [Resource Groups](https://docs.microsoft.com/en-gb/azure/azure-resource-manager/management/overview#resource-groups)
- [Regions and geographies](https://azure.microsoft.com/en-gb/global-infrastructure/geographies/#overview)
- [`az group` commands](https://docs.microsoft.com/en-us/cli/azure/group?view=azure-cli-latest)
- [JMESPath JSON query language](http://jmespath.org/)


## Create a new RG in the portal

Open https://portal.azure.com and sign in if you need to. 

Select _Create a Resource_ from the _Azure services_ section, search for Resource Groups and create a new one.

- call it `labs-rg-1`
- select a region near to you (note the list is split between _Recommended_ and _Others_)
- add a tag: `courselabs=azure`
- click create and watch for an alert to say the resource is ready
- browse to the Resource Group and explore the UI

> Each region is a collection of nearby data centres. Typically you put all the components for an app into the same region, for minimal network latency. You may put additional deployments in other regions for high availability.

You can't do much with a Resource Group on its own, but we'll always create an RG to house other resources.

## Create an RG with the Azure CLI

You manage Resource Groups with the `az group` commands. Print the help to see what's available:

```
az group --help
```

📋 Print the help text for creating a new RG. What parameters do you need to supply?

<details>
  <summary>Not sure?</summary>

Help applies for groups of commands and individual commands:

```
az group create --help
```

The only required parameters are the group name and the region - which is also referred to as the location.

</details><br/>

The CLI help text shows you how to find the list of regions too. 

📋 Create a new RG called `labs-rg-2` in a different region from the first, with the same tag `courselabs=azure`.

<details>
  <summary>Not sure how?</summary>

Find the list of regions (this command is in the `group create` help text):

```
az account list-locations -o table
```

Create a group, this example uses West US 2:

```
az group create -n labs-rg-2 -l westus2 --tags courselabs=azure
```

</details><br/>

When you create a resource with the CLI it waits until resource is ready and then prints the details.

## Manage Resource Groups

The `az` command line works in a consistent way for all resources. You create, list, show and delete them using the same verbs.

📋 Print the list of all your RGs, showing the output in table form.

<details>
  <summary>Not sure how?</summary>

```
az group list -o table 
```

</details><br/>

We added the same tag to both RGs. Tags are simple key-value pairs which you can add to all resource to help manage them. You might have an `environment` tag to identify resources in dev or UAT environments.

You can add a query parameter to `list` commands to filter the results. Complete this query to print RGs which have the matching tag:

```
az group list -o table --query "[?tags ...
```

> The query parameter uses [JMESPath](http://jmespath.org/), a JSON query language. Results find all matching RGs across all regions.

## Delete Resource Groups

The `group delete` command removes a Resource Group - and any resources inside that group. You can have an RG with five Hadoop clusters and hundreds of Docker containers, and deleting the group will stop and remove the services and delete the data.

Because resource deletion is dangerous, the `az` command doesn't let you delete multiple groups based on a query. Try this - it will fail:

```
# this will produce an error saying a group name is needed:
az group delete --query "[?tags.courselabs=='azure']"
```

📋 Delete the first resource group `labs-rg-1` using the command line.

<details>
  <summary>Not sure how?</summary>

```
az group delete -n labs-rg-1
```

</details><br/>

> You'll be asked for confirmation and then the command will wait until the group is deleted.

## Lab

Sometimes you do want to delete all the resources that match in a query. How can you delete all the RGs with the courselabs tag with a single command?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).