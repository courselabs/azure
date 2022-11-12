# Project 2: Distributed Apps

The projects are your chance to spend some dedicated time with Azure, designing and deploying a solution of your own.

You'll use all the key skills you've learned, and:

- 😣 you will get stuck
- 💥 you will have errors and broken apps
- 📑 you will need to research and troubleshoot

**That's why the projects are so useful!** 

They will help you understand which areas you're comfortable with and where you need to spend some more time.

This second project is a _distributed application_. That's an architecture where there are multiple components which work together to provide the full functionality. This app is an evolution of [Project 1](/projects/lift-and-shift/README.md), which is using a more advanced architecture to take better advantage of the cloud.

## Application Architecture

The UI for the application is the same:

![Project 2 app](/img/project-1-app.png)

This version of the app has multiple components:

![Project 2 architecture](/img/project-2-arch.png)

- a user-facing web application (.NET 6)
- a transactional database (SQL Server)
- a message queue (Azure Service Bus)
- a message handler (.NET 6)
- a document database for centralized logging (Azure Table Storage) 

## 🥅 Goals

The goal is to run the app in Azure **with a secure deployment**. 

All we have is the source code and some documentation about the configuration settings, and that should be all we need. But if you finished Project 1 you can use that a starting point.

Now that the app is distributed there's a bigger attack surface for hackers. When you have the app running the next stage is to lock it down:

- using a secure storage component for sensitive configuration data
- restricting access so the infrastructure (storage and queues) cannot be used outside of our own application components

The secure application should have a fully automated deployment. 

**You should be able to bring the application up from nothing by running a single command.**

## Remember...

_Explore | Deploy | Automate_

There are a lot of moving parts here. It would be sensible to get the app fully running in Azure before you add the security configuration.

## Dev Environment

It's also a good idea to run the application locally first, so you get a feel for the new architecture. It relies on Azure components which you will need to create first:

- a SQL Server database
- a Service Bus queue called `events.todo.newitem`
- a Table Storage service

You'll also need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download) installed on your machine

Get the connection strings for those services, then run the backend message handler:

```
cd projects/distributed/src/save-handler

dotnet run `
 --ConnectionStrings:ToDoDb='<sqlserver-connection-string>' `
 --ConnectionStrings:ServiceBus='<servicebus-connection-string>' `
 --Serilog:WriteTo:0:Args:connectionString='<tablestorage-connection-string>'
```

> This runs the message handler, it listens on the queue waiting for messages to process. Next you need to run the website.

You'll only see a couple of configuration log entries in the terminal for the message handler. Then check your Table Storage and you should see a log saying the handler has subscribed to the queue.

Now open a new terminal to run the website:

```
cd projects/distributed/src/web

dotnet run `
 --ConnectionStrings:ToDoDb='<sqlserver-connection-string>' `
 --ConnectionStrings:ServiceBus='<servicebus-connection-string>' `
 --Serilog:WriteTo:0:Args:connectionString='<tablestorage-connection-string>'
```

Browse to http://localhost:5000. When you can see the app, add a new item.

---
🤔 **YOU WON'T SEE YOUR NEW ITEM IN THE LIST** 😟

It gets added in the background and the web page loads the list before the handler has got the message and inserted the new data. Refresh the list and then you should see your new item.

---

Check Table Storage again and you should see more logs. Both components are logging to a central store, which makes debugging much easier.

## Configuration 

The application uses the standard .NET configuration model. The default settings are in 
[web/appsettings.json](/projects/distributed/src/web/appsettings.json) and [save-handler/appsettings.json](/projects/distributed/src/web/appsettings.json) . **You should not change those files**. They have the correct settings for running in dev, and that's what we want in the source code repo.

When you deploy to Azure you will need to set the same configuration items you used locally. Both the website and message handler are able to read connection strings from KeyVault, but you will need to enable that with two app settings:

- `KeyVault__Enabled` set to `True`
- `KeyVault__Name` set to the KeyVault name

The format of the connection string keys is different depending on where you store them:

|| Application Setting Name | KeyVault Secret Name | 
|-|-|-|
|SQL Server | `ConnectionStrings__ToDoDb` | `ConnectionStrings--ToDoDb`|
|Service Bus | `ConnectionStrings__ServiceBus` | `ConnectionStrings--ServiceBus`|
|Table Storage | `Serilog__WriteTo__0__Args__connectionString` | `Serilog--WriteTo--0--Args--connectionString`|

## Source Code

The code for the app is in this repo:

- `projects/distributed/src/web` - the website 
- `projects/distributed/src/save-handler` - the background message handler

**The message handler has also been compiled into a deployment package:**

- `projects/distributed/src/save-handler/deploy.zip` - this is in the correct format to deploy as a webjob in a Windows App Service on Azure, if that's how you're planning to run it

## Stretch

This architecture is all about reliability and scale. What happens if you run multiple instances of the message handler? What happens if there are no message handlers running - do the items get inserted when one starts again?

It's a shame there's a lag when you add a new item. That's called _eventual consistency_ and it's one of the drawbacks of a distributed architecture. You can't solve that with Azure, it would need code changes.

