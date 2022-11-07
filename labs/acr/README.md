# Azure Container Registry

Open source applications are often published as container images on Docker Hub. Services which host images are called container registries, and you'll want to use a private registry for your own apps rather than a public one. Azure Container Registry is the service you use to create and manage your own registry. It integrates with Azure security and lets you store images in the same region as the service where you'll run containers.

## Reference

- [Docker Hub overview](https://docs.docker.com/docker-hub/)

- [Container Registry documentation](https://docs.microsoft.com/en-gb/azure/container-registry/)

- [`az acr` commands](https://docs.microsoft.com/en-us/cli/azure/acr?view=azure-cli-latest)


## Explore ACR in the Portal

Open the Portal and search to create a new Container Registry resource. Switch through the different SKUs and look at the options you have:

- private networking and customer-managed encryption keys are available with the Premium SKU
- the registry name becomes the DNS name, with the `.azurecr.io` suffix, so it needs to be globally unique.

Back to the terminal now to create a registry with the command line.

## Create an ACR instance with the CLI

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-acr --tags courselabs=azure -l eastus
```

📋 Create a new registry with the `acr create` command. You'll need to find a unique name for it.

<details>
  <summary>Not sure how?</summary>

Start with the help:

```
az acr create --help
```

There are a lot more options than you see in the Portal. If you do use the Portal to create a registry you can set a lot of these in the management page.

This creates a Basic-SKU registry:

```
az acr create -g labs-acr -l eastus --sku 'Basic' -n <acr-name>
```

ACR names are stricter than most, so you might see an error if you try to use an illegal character.

</details><br/>

When the command completes you have your own registry, available at the domain name `<acr-name>.azurecr.io` - you'll see the full name in the `loginServer` field in the output.

## Pull and Push Images to ACR

Docker image names can include a registry domain. The default registry is Docker Hub (`docker.io`) so you don't need a domain for that - the full name for the image `nginx:alpine` is actually `docker.io/nginx:alpine`.

Pulling an image downloads the latest version:

```
docker image pull docker.io/nginx:alpine
```

You can upload a copy of that image to ACR, but you need to change the name to use your ACR domain instead of Docker Hub. The `tag` command does that:

_Make sure you use **your** ACR domain name:_

```
docker image tag docker.io/nginx:alpine <acr-name>.azurecr.io/labs-acr/nginx:alpine-2204
```

> You can change all parts of the image name with a new tag.

Now you have two tags for the Nginx image:

```
docker image ls --filter reference=nginx --filter reference=*/labs-acr/nginx
```

Your ACR tag and the Docker Hub tag both have the same image ID; tags are like aliases and one image can have many tags.

You upload images to a registry with the `push` command, but first you need to authenticate.

_Try pushing your image to ACR:_

```
# this will fail:
docker image push <acr-name>.azurecr.io/labs-acr/nginx:alpine-2204
```

📋 You can authenticate to the registry with your Azure account. Log in with an `az acr` command and then push the image.

<details>
  <summary>Not sure how?</summary>

List the ACR commands:

```
az acr --help
```

You'll see there's a `login` command which just needs your ACR name:

```
az acr login -n <acr-name>
```

Now when you push your image it will upload:

```
docker image push <acr-name>.azurecr.io/labs-acr/nginx:alpine-2204
```

</details><br/>

You can run a container from that image with this command:

```
docker run -d -p 8080:80 <acr-name>.azurecr.io/labs-acr/nginx:alpine-2204
```

You can browse the app at http://localhost:8080. It's the standard Nginx app, but it's available from your own image registry. Anyone who has access to your ACR can run the same app from the image.

## Build and Push a Custom Image

When you build an image you can include the registry domain in the tag. Run this to build the simple ASP.NET web app from the [Docker 101 lab](/labs/docker/README.md), including a version number in the image tag:

```
docker build -t  <acr-name>.azurecr.io/labs-acr/simple-web:6.0 src/simple-web
```

> It will build very quickly if you've built this image before, because Docker does a lot of caching

📋 Create another tag for the same image, this time using `latest` as the version number instead of `6.0`.

<details>
  <summary>Not sure how?</summary>

You can use the `tag` command to create a new tag for an image and change any part of the name, including the registry domain or verion number:

```
docker tag <acr-name>.azurecr.io/labs-acr/simple-web:6.0 <acr-name>.azurecr.io/labs-acr/simple-web:latest
```

</details><br/>

List all of the images tagged with your ACR domain:

```
docker image ls <acr-name>.azurecr.io/*/*
```

You'll have two versions of your `simple-web` image. You can push both versions with one command:

```
docker push --all-tags <acr-name>.azurecr.io/labs-acr/simple-web
```

## Browse to ACR in portal  

ACR is one service which can be easier to manage in the Portal than with the command line. 

Browse to your ACR and open the _Repositories_ list. You'll see the images you pushed - what do the tags and manifests mean? Check out the other ACR features, webhooks and replications are things you might use.

## Lab

If you use containers with Azure you might have a CI job which builds and pushes images to ACR every time there's a code change. You're charged for storage with ACR so you might want a script that can clean up old images on a schedule.

Look at how you can delete images with the `az` command, and if scripting is your thing see if you can write a script which will delete all but the 5 most recent image versions.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the Azure resources, including your ACR instance and its images:

```
az group delete -y --no-wait -n labs-acr
```

And run this command to remove all your local Docker containers:

```
docker rm -f $(docker ps -aq)
```