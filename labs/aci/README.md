# Azure Container Instances

The great thing about Docker containers is they're portable - your app runs in the same way on Docker Desktop as it does on any other container runtime. Azure offers several services for running containers, and the simplest is Azure Container Instances (ACI) which is a managed container service. You run your apps in containers and you don't have to manage any of the underlying infrastructure.

## Reference

- [Container Instances documentation](https://docs.microsoft.com/en-gb/azure/container-instances/)

- [`az container` commands](https://docs.microsoft.com/en-us/cli/azure/container?view=azure-cli-latest)


## Explore Azure Container Instances

Open the Portal and search to create a new Container Instance resource. Look at the options available to you:

- the image registry to use - it could be your own ACR instance or a public registry like Docker Hub
- the container image to run
- the compute size of your container - number of CPU cores and memory
- in the networking options you can publish ports and choose a DNS name to access your app
- in the advanced options you can set environment variables for the container

You can run Linux and Windows containers with ACI, so you can run new and old applications. The UX is the same - we'll see how the service works using the command line.

## Create an ACI container with the CLI

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-aci --tags courselabs=azure -l eastus
```

Now you can use the `az container create` command to run ACI instances in the RG.

📋 Create a new container called `simple-web` to run the image `courselabs/simple-web:6.0` on Docker Hub. Publish port `80` and include a DNS name in your command so you'll be able to browse to the app running in the container.

<details>
  <summary>Not sure how?</summary>

Start with the help:

```
az container create --help
```

You need to use the `image` and `ports` parameters, and pass a unique prefix for the `dns-name-label`, e.g:

```
az container create -g labs-aci --name simple-web --image courselabs/simple-web:6.0 --ports 80 --dns-name-label <dns-name>
```

</details><br/>

When the command returns, the new container is running. The output includes an `fqdn` field, which is the full DNS name you can use to browse to your container app.

> Browse to the app. **It may take a couple of minutes to come online**. It's the same container image we built in the [Docker 101 lab](/labs/docker/README.md).

You can configure a lot more details in the `container create` command. How much CPU and RAM does your container have? That can't be changed when the container is running, but you could replace this container with a new one from the same image and specify the compute.

Other `az container` commands let you manage your containerized apps. 

📋 Print the application logs from your ACI container.

<details>
  <summary>Not sure how?</summary>

```
az container logs -g labs-aci -n simple-web
```

</details><br/>

You'll see the ASP.NET application logs from the container.

## Deploy to ACI from Docker

If you use containers a lot, then it's easier to stick with the familiar Docker commands. The `docker` CLI can manage containers on your local machine or on a remote environment. You can create a Docker [context](https://docs.docker.com/engine/context/working-with-contexts/) to [create and manage ACI containers with the standard Docker CLI](https://docs.docker.com/cloud/aci-integration/).

The Docker and Azure CLIs don't share credentials, so first you need to login to your Azure subscription from Docker:

```
docker login azure
```

This loads a browser window for you to authenticate - just like the `az login` command.

Now you can create a context. A Docker ACI context manages containers in a single Resource Group. The CLI will ask you to select an existing subscription and RG:

```
docker context create aci labs-aci --resource-group labs-aci
```

> If your Microsoft account has access to multiple Azure subscriptions you'll be shown a list here. Choose the subscription where you created the `labs-aci` RG.

Switch your context to point the Docker CLI to ACI:

```
docker context use labs-aci
```

📋 Use `docker` commands to list all your ACI containers and print the logs.

<details>
  <summary>Not sure how?</summary>

Not all the Docker commands work with an ACI context, but the most common ones do. Run `ps` to list all running containers:

```
docker ps
```

You'll see your ACI containers listed, including the domain name and published port(s). You can use a container ID to print the logs:

```
docker logs <container-id>
```

</details><br/>

[ACI integration container features](https://docs.docker.com/cloud/aci-container-features/) lists all the Docker commands you can use to manage ACI containers. 

📋 Run another instance of the `simple-web` container in ACI, this time using the Docker command line. Publish port `80` to a different domain name.

<details>
  <summary>Not sure how?</summary>

This is a mixture of standard Docker parameters like `ports`, and custom  parameters for ACI, like `domainname`:

```
docker run -d -p 80:80 --domainname <new-aci-domain> courselabs/simple-web:6.0
```

</details><br/>

The output will include a random name which Docker generates. List out your containers and you'll see the new Docker-created instance as well as the original created with `az`:

```
az container list -o table
```

You should be able to browse to your new container and see the same application.

> The Docker command line is an alternative way to manage containers, but they're still running in ACI in the same way as if you'd created them with the portal or the Azure CLI.

## Lab

You can migrate all your .NET apps to containers, but you'll need to use Windows containers for older .NET Framework apps. Docker Desktop on Windows supports Linux and Windows containers (you can switch from the Docker icon in the taskbar), and so does ACI.

The [simple-web image](https://hub.docker.com/r/courselabs/simple-web/tags) has been published with Windows and Linux variants. Run an ACI container from the Windows image version, how does it differ from the Linux version? Then see what happens if you try to run the Linux image which has been compiled for ARM processors instead of Intel/AMD.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources, including the containers you created with the Docker CLI:

```
az group delete -y --no-wait -n labs-aci
```

Now change your Docker context back to your local Docker Desktop, and remove the lab context:

```
docker context use default

docker context rm labs-aci
```