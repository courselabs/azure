# Project 4: Serverless Apps

The projects are your chance to spend some dedicated time with Azure, designing and deploying a solution of your own.

You'll use all the key skills you've learned, and:

- 😣 you will get stuck
- 💥 you will have errors and broken apps
- 📑 you will need to research and troubleshoot

**That's why the projects are so useful!** 

They will help you understand which areas you're comfortable with and where you need to spend some more time.

This fourth project is a _serverless application_. The solution isn't entirely serverless, but serverless functions have been written to add features and improve functionality. The app is an evolution of [Project 2](/projects/distributed/README.md), using the same architecture for the front-end, database and messaging, but with the back-end replaced with functions.

## Application Architecture

The UI for the application is the same:

![Project 4 app](/img/project-1-app.png)

This version of the app uses a similar distributed architecture as [Project 2](/projects/distributed/README.md), but the message handler has been replaced with a serverless function, and there are additional functions acting as a REST API and a notification hub:

![Project 3 architecture](/img/project-4-arch.png)

- a user-facing web application (.NET 6)
- a transactional database (SQL Server)
- a message queue (Azure Service Bus)
- a SignalR service to broadcast notifications (Azure SignalR Service)
- a serverless function for new to-do items to be created (.NET 6)
- a serverless function to save data to SQL Server (.NET 6)
- a serverless function to broadcast new-item notifications (.NET 6)

You can't run the whole application locally because the key dependencies - Service Bus and SignalR - can't be run locally. You can run the website and the functions locally and prove out most of the functionality.

## 🥅 Goals

The goal is to deploy the app to Azure.

All we have is the source code and some documentation about the configuration settings, and that should be all we need. If you finished Project 2 you can use that a starting point.

When you have the app running and tested in Azure there are two more stages:

1. expose the new REST API through an API Management tool so developers can on-board themselves
2. add centralized monitoring for the web application and the serverless functions, so we have a single place to see logs and metrics

The application should have a fully automated deployment. 

**There's a lot here but try and treat it like a real project - work on it in stages so you're always close to having something to show.**

## Remember...

_Explore | Deploy | Automate_

There are a lot of moving parts here. It would be sensible to get the app fully running in Azure before you script the deployment and move on to the next stages.

## Dev Environment

It's a good idea to try and run the application locally first, so you get a feel for the new architecture. It relies on Azure components which you will need to create in advance:

- a SQL Server database
- two Service Bus queues
    - `events.todo.newitem`
    - `events.todo.itemsaved`
- a SignalR Service instance

You'll also need a local container for the Storage Account emulator:

```
docker run -d -p 10000-10002:10000-10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

To configure the functions create the file `local.settings.json` in the folder `functions/TodoList.Functions` with these settings and your connection strings:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "ServiceBusConnectionString" : "<sb-connection-string>",
        "SqlServerConnectionString" : "<sql-connection-string>",
        "SignalRConnectionString" : "<sr-connection-string>"
    },
    "Host": {
      "CORS": "http://localhost:5000/",
      "CORSCredentials": true
    }
}
```

Now run the functions from the `functions/TodoList.Functions` folder:

```
func start
```

To configure the website edit the file `appsettings.json` in the folder `src/web` and set your connection strings:

```
"ConnectionStrings": {    
    "ToDoDb": "<sql-connection-string>",
    "ServiceBus": "<sb-connection-string>",
    "Functions": "http://localhost:7071/api"
  }
```

In another terminal run the website from the `src/web` folder:

```
dotnet run
```

Browse to http://localhost:5000.  When you can see the To-Do website, add a new item.

---
🤔 **YOU WON'T SEE YOUR NEW ITEM IN THE LIST** 😟

The functions emulator doesn't fully support SignalR negotiation (you'll see a CORS issue in the browser developer tools). But if you refresh and the data is there, then the functions are firing correctly through Service Bus and saving data to SQL Server.

---

You can also add to-do list items with the new REST API:

```
# on Windows use curl.exe
curl -XPOST http://localhost:7071/api/items --header 'Content-Type: text/plain' --data-raw 'a new item'
```

Refresh the website and you'll see the new item.

> When you deploy to Azure the full SignalR flow through Functions is supported if you add your website URL as an allowed origin in CORS, and enable request credentials.

## Configuration 

When you deploy to Azure you will need to set the same configuration items you used locally - both for the web application and the functions.

The format of the configuration keys is different for each component:

|| Web App Setting Name | Functions App Setting Name | 
|-|-|-|
|SQL Server | `ConnectionStrings__ToDoDb` | `SqlServerConnectionString`|
|Service Bus | `ConnectionStrings__ServiceBus` | `ServiceBusConnectionString`|
|SignalR Service| n/a | `SignalRConnectionString`|
|Functions| `ConnectionStrings__Functions` | n/a |

## Source Code

The code for the app is in this repo:

- `projects/serverless/src` - website and supporting projects
- `projects/serverless/functions` - serverless functions

## Stretch

In the unlikely event that you get the app deployed, with API Management configured and centralized monitoring, and have time to spare we need to think about one other thing - _security_:

- the infrastructure components (database and message queue) should only be accessible by the web app and functions

- the API function should only be accessible by API Management

- the web app should read configuration settings from KeyVault (the app can do this - you can set it up the same way as [Project 2](/projects/distributed/README.md))

