# Azure Functions with Containers

fn with container runtime - deployment model is image, build and push. need to use a specific image base & always on hosting plan (not serverless)


## Reference

- [Azure Functions with Docker containers](https://learn.microsoft.com/en-us/azure/azure-functions/functions-create-function-linux-custom-image?tabs=in-process%2Cbash%2Cazure-cli&pivots=programming-language-csharp)

- [Functions base images](https://hub.docker.com/_/microsoft-azure-functions-base)


## Explore Container Functions in the Portal

Create new Function App in the Portal:

- select lab RG
- select _Docker container_ as the runtime
- create a new SA 
- disable application insights
- check the _Docker_ page

Are you deploying an application here? Nowhere to choose your image - remember the Function App is the hosting component. With the container runtime you deploy a sample app to get the app provisioned.

Strange UX - stick to the CLI.

## Create Resource Group and ACR

```
az group create -n labs-functions-containers --tags courselabs=azure -l eastus
```


ACR needs standard Docker creds to use with functions:

```
az acr create -g labs-functions-containers -l eastus --sku Basic --admin-enabled true -n <acr-name>
```

## Build and push application images

build - custom docker image & standard API function code:

- api dockerfile
- fnd function

```
cd src/rng/Numbers.Api.Function

docker build -t <acr-name>.azurecr.io/rng/api:1.0.0 . 
```
> If you don't have an Intel machine (e.g. Mac with M1 or M2) this will fail, because the base image is not available for ARM.
Use my image on Docker Hub instead: `courselabs/rng-api:fn-4-dotnet-6`

get creds & login:

(could also use `az acr login -n <acr-name>`, but this verifies creds for fn)

```
az acr credential show -g labs-functions-containers -n <acr-name>

docker login <acr-name>.azurecr.io -u <username>
```

> **Keep this safe**

push

```
docker push <acr-name>.azurecr.io/rng/api:1.0.0
```


Browse to acr in portal - check images are there and credentials there for Docker login (admin account under _Access Keys_)


## Deploy random number containers as Functions


Pre-reqs - SA & function app:

```
az storage account create  -g labs-functions-containers --sku Standard_LRS -l eastus -n <sa-name> 

az functionapp plan create -g labs-functions-containers --number-of-workers 1 --sku EP1 --is-linux -l eastus -n <fn-plan-name> 
```

API container:

```
az functionapp create -g labs-functions-containers --name <rng-api-name> --storage-account <sa-name> --plan <fn-plan-name>  --deployment-container-image-name <acr-name>.azurecr.io/rng/api:1.0.0 --docker-registry-server-user <acr-username> --docker-registry-server-password <acr-password>
```

Needs access to storage for logs etc:

```
az storage account show-connection-string -g labs-functions-containers --query connectionString -o tsv -n <sa-name> 

az functionapp config appsettings set -g labs-functions-containers --name <rng-api-name> --settings AzureWebJobsStorage='<sa-connection-string>'
```

Check the function in the portal - open _Deployment Center_; you have the option to use MI instead of admin creds, but not in the CLI(?); logs will show the image being pulled and the container starting



## Lab

