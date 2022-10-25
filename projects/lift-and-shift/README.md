# Project 1: Lift and Shift

The projects are your chance to spend some dedicated time with Azure, designing and deploying a solution of your own.

You'll use all the key skills you've learned, and:

- 😣 you will get stuck
- 💥 you will have errors and broken apps
- 📑 you will need to research and troubleshoot

**That's why the projects are so useful!** 

They will help you understand which areas you're comfortable with and where you need to spend some more time.

This first project is a _lift and shift_. That's where you have an existing application you want to run in the cloud, without changing the architecture or the code. It's often the first approach companies take when they're getting started in the cloud.

## Application Architecture

Our project app is a simple to-do list application. It's web-based, so you can browse to it and add the list of things you need to get done:

![Project 1 app](/img/project-1-app.png)

This version of the app has two components, a web application and a backend database:

![Project 1 architecture](/img/project-1-arch.png)

## 🥅 Goals

The overall goal is to run the app in Azure. 

All we have is the source code and some documentation about the configuration settings, and that should be all we need.

What we really want is:

- a test environment with a small database and a single app instance
- a *separate* production environment with a larger database and multiple app instances

Both environments need to have fully automated deployments. **You should be able to bring the application up from nothing by running a single command.**

You can choose which Azure services to use and how to model the deployments.

## Remember...

_Explore | Deploy | Automate_

## Dev Environment

It's a good idea to run the application locally first, so you get a feel for how it looks. 

> You'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download) installed

Open the source code folder in a terminal session and run the app:

```
cd projects/lift-and-shift/src

dotnet run
```

You'll see some output in the logs.

- browse to the app at http://localhost:5000
- the dev deployment uses a local database, so you don't need SQL Server running
- check you can add to-do items and they get shown in the list
- open the diagnostics page to see what information it gives you

## Configuration 

The application uses the standard .NET configuration model. The default settings are in the 
[appsettings.json](/projects/lift-and-shift/src/appsettings.json) file. **You should not change that file**. It has the correct settings for running in dev, and that's what we want in the source code repo.

When you deploy to Azure you will need to change some settings in the cloud environment:

- `Database__Provider` needs to be `SqlServer`
- `ConnectionStrings__ToDoDb` needs the full connection string to your SQL Server database
- `ConfigController__Enabled` can be set to `true`; if it is then the app will expose a /config page which you can use to check the loaded config

## Testing

When you have the app running in Azure, you should be able to:

- browse to the URL and see the app with no errors
- add to-do items and see them in the list
- query the to-do items in SQL Server

## Stretch

If you get everything done, then think about how you could extend your deployment to make it more production-grade. Scale, high availabilty and the rollout process for application upgrades are all worth considering.