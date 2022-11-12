# Distributed Apps on Azure Container Instances

ACI is the simplest container platform on Azure. You can run single containers, or you can run multiple containers in a group to host a distributed application. There are different options for modelling the applications - you can use Azure's YAML spec with the Azure CLI, or the Docker Compose spec with the Docker CLI.

In this lab we'll use both options and see how ACI integrates with other Azure services.

## Reference

- [ACI container groups overview](https://learn.microsoft.com/en-us/azure/container-instances/container-instances-container-groups)

- [ACI YAML specification](https://learn.microsoft.com/en-us/azure/container-instances/container-instances-reference-yaml)

- [Docker ACI integration](https://docs.docker.com/cloud/aci-integration/)

- [ACI Compose features](https://docs.docker.com/cloud/aci-compose-features/)

- [`az container` commands](https://docs.microsoft.com/en-us/cli/azure/container?view=azure-cli-latest)

## Deploy a distributed app with ACI YAML

Start by creating a Resource Group for the lab:

```
az group create -n labs-aci-compose --tags courselabs=azure -l eastus
```

The YAML model for ACI is a custom format which looks a bit like Bicep and a bit like Compose but isn't either:

- [rng-aci-v1.yaml](/labs/aci-compose/rng-aci-v1.yaml) - defines a _container group_ with two containers for the random number API and website images we've used before

There are some specific details we need to include in this model:

- container sizes (CPU & memory) are required, so the ACI can provision compute
- a group of containers share the same network space, so the environment variables configure communication via `localhost`
- any public services need to have ports specified at the IP address level and the container level

You can pass this model to the Azure CLI when you create an ACI resource, and that will run all the containers in the model.

📋 Deploy the app with a `container create` command and the file in `labs/aci-compose/rng-aci-v1.yaml`.

<details>
  <summary>Not sure how?</summary>

Check the help text:

```
az container create --help
```

You can supply a `file` parameter and a name:

```
az container create -g labs-aci-compose -n rng-app --file labs/aci-compose/rng-aci-v1.yaml
```

</details><br/>

Open the ACI resource in the Portal. Under _Containers_ you should see the API and web containers both running. You can see the properties and logs for each container, and even connect to a shell session inside a container if you need to debug.

The YAML spec didn't include a DNS name, but you'll see there's a public IP address we can use to try the app. 

> Browse to `http://<aci-ip-address>` and click the button - you should see a random number 

The logs from the API container don't give us much detail, but we can increase the logging level with a change to the model:

- [rng-aci-v2.yaml](/labs/aci-compose/rng-aci-v2.yaml) - adds a new environment variable to each container so we can see more logs

📋 Deploy the updated spec in `labs/aci-compose/rng-aci-v2.yaml`. This is just a configuration change, how does ACI actually implement the update?

<details>
  <summary>Not sure how?</summary>

Use the same container create command with the instance name and the updated spec:

```
az container create -g labs-aci-compose -n rng-app --file labs/aci-compose/rng-aci-v2.yaml
```

You'll see the output `Running...` for a while. The command recreates the containers and waits for the new ones to come online.

</details><br/>

Check in the _Events_ table for the containers in the Portal and you will see multiple entries for containers _Started_ and entries for _Killing_ the old containers.

You can't change any properties of the compute environment for a running container. If you need to update environment variables, resource requests or ports then the only way to do that is by removing the old container and creating a replacement.

> That's true for all container runtimes - Docker, ACI and Kubernetes 

ACI has its own YAML specification so you have access to all the features. If you don't need ACI-specific configuration then you can model your app in a standard Docker Compose file instead and deploy to ACI with the Docker CLI.

## Deploy a Compose App to ACI

The Compose model for the app is much simpler:

- [rng-compose-v1.yml](/labs/aci-compose/rng-compose-v1.yml) - still uses the same container images, but the Compose integration takes care of some of the differences in ACI

We can deploy to ACI using a `docker compose` command, but first we need to set up a Docker Context so our local CLI is configured to talk to Azure (we covered this in the [ACI lab](/labs/aci/README.md)):

```
docker login azure

docker context create aci labs-aci-compose --resource-group labs-aci-compose

docker context use labs-aci-compose
```

Now when you run `docker` and `compose` commands you're working in the context of ACI in your lab resource group:

```
# this will show the containers you deployed with the az command:
docker ps
```

📋 Use the `docker compose` command to bring the application up from the file `labs/aci-compose/rng-compose-v1.yml`.

<details>
  <summary>Not sure how?</summary>

It's the usual `up` command - you can specify a project name which becomes the ACI name:

```
docker compose -f labs/aci-compose/rng-compose-v1.yml --project-name rng-app-2 up -d 
```

</details><br/>

> You'll see output about the group being created, then the individual containers are created in parallel.

You can see your new containers in the Portal, or you can use the Docker command to print the details:

```
# for ACI containers the output includes the IP address:
docker ps
```

Browse to the new deployment and check the app is working. Open the container list in the Portal - you'll see there are three even though we only define two in the model. What might the additional container be doing?

## ACI containers and Storage Accounts

Create Storage Account:

```
az storage account create --sku Standard_ZRS -g labs-aci-compose  -l westeurope -n <sa-name>
```

Get the connection string:

```
az storage account show-connection-string -g labs-aci-compose --query connectionString -o tsv -n <sa-name>
```

Use it to run a container locally with Blob Storage as the database:

```
# switch to the local Docker engine:
docker context use default

# be careful with the 'quotes' - they start at the key and end at the value:
docker run --name local -d -p 8013:80 -e 'ConnectionStrings__AssetsDb=<connection-string>' courselabs/asset-manager:22.11
```

Browse to http://localhost:8013 and you'll see some data on the screen. Open your Blob Storage container in the Portal and there's the raw data, uploaded from your local container.

The container also writes a file to local storage - this doesn't really do anything, but it uses the container name as the filename:

```
# list the contents of the folder in the container:
docker exec local ls /app/lockfiles
```

An ACI container can access Blob Storage using the same code, and you can also mount an Azure Files share in ACI. The share appears as part of the container filesystem, but when the app writes data there it's actually stored in the share.

📋 Create a file share called `assetmanager` in your Azure Storage Account and print the Storage Account key.

<details>
  <summary>Not sure how?</summary>

We did this in the [Azure Files lab](/labs/storage-files/README.md):

```
# create the share:
az storage share create -n assetmanager --account-name <sa-name>

# print the key:
az storage account keys list -g labs-aci-compose --query "[0].value" -o tsv --account-name <sa-name>
```

</details><br/>

**Edit the file** [assetmanager-aci.yaml](/labs/aci-compose/assetmanager-aci.yaml):

- replace `<sa-name>` and `<sa-key>` with your own Storage Account name and key
- replace `<connection-string>` with your Storage Account connection string

Then deploy a new ACI group to run the Asset Manager container using Blob Storage and Files:

```
az container create -g labs-aci-compose --file labs/aci-compose/assetmanager-aci.yaml
```

Browse to the app when it's running. Can you find the lock file in your Storage Account?

## Lab

ACI is meant to be a quick and easy solution for running containers in the cloud. It's reliable because it will restart containers if they fail, but it doesn't have a feature to let you scale horizontally. Run another copy of the Asset Manager app in ACI. Do they both write to the same file share? How would you load-balance traffic between them?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources, including the containers you created with the Docker CLI:

```
az group delete -y --no-wait -n labs-aci-compose
```

Now change your Docker context back to your local Docker Desktop, and remove the lab context:

```
docker context use default

docker context rm labs-aci-compose
```