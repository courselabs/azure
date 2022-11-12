# Project 3: Containerized Apps

The projects are your chance to spend some dedicated time with Azure, designing and deploying a solution of your own.

You'll use all the key skills you've learned, and:

- 😣 you will get stuck
- 💥 you will have errors and broken apps
- 📑 you will need to research and troubleshoot

**That's why the projects are so useful!** 

They will help you understand which areas you're comfortable with and where you need to spend some more time.

This third project is a _containerized application_. That's an application which is designed to run in containers. This app is an evolution of [Project 2](/projects/distributed/README.md), which is using the same architecture but different technologies, to make a solution which is portable between clouds and other environments.

## Application Architecture

The UI for the application is the same:

![Project 3 app](/img/project-1-app.png)

This version of the app the same distributed architecture as [Project 2](), but now the message queue has been swapped from Azure Service Bus to Redis:

![Project 3 architecture](/img/project-3-arch.png)

- a user-facing web application (.NET 6)
- a transactional database (SQL Server)
- a message queue (Redis)
- a message handler (.NET 6)
- a document database for centralized logging (Azure Table Storage) 

Using Redis makes the application more portable. All the components can be run in containers, or the infrastructure components (database, message qeue and table storage) can be in managed services.

## 🥅 Goals

The goal is to run the app in AKS. 

We already have a working version of the app using containers, but modelled with Docker Compose. The Compose implementation has all the information you need to build your Kubernetes model.

When you have the app modelled and running locally there are three more stages:

1. run a development environment in AKS, using containers for all components
2. run a production environment in AKS, using Azure services for the infrastructure components
3. secure the production Azure deployment, so the infrastructure services can only be used from AKS

The application should have a fully automated deployment. 

**There is a lot to do in this project - however far you get is good work.**

## Remember...

_Explore | Deploy | Automate_

There are a lot of components to model and wire up correctly. It would be sensible to get the app fully running in Kubernetes locally on Docker Desktop before you move on to AKS.

## Dev Environment

You don't need any Azure resources needed to run this version of the app locally. 

[docker-compose.yml](/projects/containerized/docker-compose.yml) - models the app to run in Docker using:

- images from Docker Hub for the web application and the message handler
- the official Redis image for messaging
- a lightweight SQL Server image for the database
- the [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=docker-hub) storage account emulator for Table Storage

```
# navigate to the project root:
cd projects/containerized

# run the app:
docker compose up -d
```

Browse to http://localhost:8099. When you can see the To-Do website, add a new item.

---
🤔 **YOU WON'T SEE YOUR NEW ITEM IN THE LIST** 😟

It still gets added in the background and the web page loads the list before the handler has got the message and inserted the new data. Refresh the list and then you should see your new item.

---

Logs are being stored in the local table storage emulator. If you want to see them you can download the [Storage Explorer](https://learn.microsoft.com/en-us/azure/vs-azure-tools-storage-manage-with-storage-explorer) and connect to the local development storage.

## Configuration 

The application uses the standard .NET configuration model. The default settings are in 
[web/appsettings.json](/projects/containerized/src/web/appsettings.json) and [save-handler/appsettings.json](/projects/containerized/src/save-handler/appsettings.json) . **You should not change those files**. They have the correct settings for running in dev, and that's what we want in the source code repo.

When you deploy to Azure you will need to set the same configuration items we set in the Compose model. Compose uses this settings file: [docker/config.json](/projects/containerized/docker/config.json) but the model will need to be different in Kubernetes.

## Source Code

We're deploying from container images on Docker Hub, so you don't need to compile the application or build any images yourself. For reference the code for the app is in this repo:

- `projects/containerized/src` - .NET source code for the web app, message handler and shared libraries
- `projects/containerized/docker` - Dockerfiles for the web app and message handler

## Stretch

This version of the app doesn't have the logic to read configuration data from KeyVault, but you can use an AKS setup which lets you mount KeyVault secrets into containers. 

Extend your deployment scripts to populate connection strings in KeyVault, and load the values from there into the container configuration.