# Azure SignalR Service

SignalR is a technology for two-way communication over the internet. It's a way for web applications to push updates to the browser, which supports asynchronous delivery to the front end. SignalR is a server technology you can run in your own application (it's Microsoft's customization of [WebSockets](https://en.wikipedia.org/wiki/WebSocket) for .NET), but each server keeps its own list of connected clients, so it's hard to scale if you run it yourself. 

Azure SignalR Service moves SignalR into its own component, so your web application doesn't deal with clients directly, it just sends an update notificaction to the SignalR Service, which is a central component that broadcasts to all clients.

In this lab we'll run a simple SignalR application locally and see how to integrate it with SignalR Service in Azure.

## Reference

- [SignalR overview](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-7.0)

- [Azure SignalR Service overview](https://learn.microsoft.com/en-us/azure/azure-signalr/signalr-overview)

- [`az signalr` commands](https://learn.microsoft.com/en-us/cli/azure/signalr?view=azure-cli-latest)

## Run a Local SignalR Website

We have a basic chat applicaion which uses SignalR to broadcast messages to connected clients. The code isn't too interesting, so we'll get straight to running it locally (you'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download)):

```
dotnet run --project src/signalr/chat --urls=http://localhost:5005/
```

Now open **two** browser windows to http://localhost:5005/ - the app asks for a username and it generates a random default. Exchange messages and you'll see both browsers update without any client-side refreshing. When a new message gets posted, the server broadcasts it using SignalR.

> Now end the server app with Ctrl-C/Cmd-C - what happens on the browser?

Restart the app with the same command:

```
dotnet run --project src/signalr/chat --urls=http://localhost:5005/
```

You can reconnect the browser with F5, but the previous messages are lost. The app doesn't have a persistence layer for storing data, and SignalR doesn't provide that.

What if you run at scale? You can run another copy of the website using a different port, but it's a completely separate instance:

```
dotnet run --project src/signalr/chat --urls=http://localhost:5006/
```

Open one browser page to each site and try sending messages:

- http://localhost:5005/ 
- http://localhost:5006/

Messages aren't shared between the two servers, so users see a different set of messages depending on which server they're connected to. This is the use-case for Azure SignalR Service - we can run as many web hosts as our application needs without having to manage client connections.

## Create SignalR Service

Open the Azure Portal, create a new resource and search for 'signalr'. Select _SignalR Service_ & create:

- it has the usual DNS name, region, and resource group requirements
- pricing tier defaults to Premium - there are also Free & Standard (with different levels of scale & reliability)
- follow the links to find out about _Units_ and _Service Mode_

> SignalR is one of the less well-documented services :)

There's no vnet integration, because SignalR Service runs on a shared platform (like App Services).

We'll create a new instance with the CLI:

```
az group create -n labs-signalr -l eastus --tags courselabs=azure 

az signalr create -g labs-signalr --sku Free_F1 -l eastus -n <signalr-name>
```

> You may see a message _Resource provider 'Microsoft.SignalRService' used by this operation is not registered. We are registering for you._

SignalR is one of the less common services, so it probably won't be enabled in your subscription by default. The CLI will set it up for you.

When it's created, browse to the  new service in the Portal:

- under _Keys_ you can see the connection string to use with an access key
- in _Connection strings_ you can find a different connection string to use with Managed Identities

We can use different authentication methods from the web server to the SignalR service.

Grab the _Connection string for Access Key_ and you can use the hosted SignalR Service with your app running locally. Run two instances again, this time passing in the configuration settings for the SignalR Service:

```
dotnet run --project src/signalr/chat --urls=http://localhost:5005/ --Azure:SignalR:Enabled=true --Azure:SignalR:ConnectionString='<signalr-connection-string>'

dotnet run --project src/signalr/chat --urls=http://localhost:5006/ --Azure:SignalR:Enabled=true --Azure:SignalR:ConnectionString='<signalr-connection-string>'
```

Now open one browser to each server again:

- http://localhost:5005/ 
- http://localhost:5006/

Can you exchange messages this time? What happens if you stop one server and restart - are the messages preserved this time?

> SignalR is for real-time broadcasting; if you want your app to maintain state then you need to take care of that in your own code

## Deploy SignalR Website to Azure

The SignalR service supports authentication with Managed Identities. We can run the website in an App Service with a System-Managed Identity and connect to SignalR without needing sensitive data in the connection string.

📋 Deploy the application from the folder `src/signalr/chat` using the `az webapp up` command.

<details>
  <summary>Not sure how?</summary>

Start with the help:

```
cd src/signalr/chat

az webapp up -g labs-signalr --os-type Linux --sku B1 --runtime dotnetcore:6.0 -n <app-name>
```

</details><br/>

Now set the SignalR connection string as a configuration app setting. We'll use a Managed Identity, so your connection string just needs the SignalR domain name and not the key:

```
az webapp config appsettings set --settings Azure__SignalR__Enabled='true' Azure__SignalR__ConnectionString='Endpoint=https://<signalr-name>.service.signalr.net;AuthType=azure.msi;Version=1.0;' -g labs-signalr -n <app-name>
```

> Browse to the app. It won't work - can you see the problem in the logs?

The page loads but message sends fail - if you open the developer tools in your browser you'll see 500 errors. Check the _Log stream_ in the Web App and you'll see something like this:

```
2022-11-11T15:10:37.631828591Z info: Microsoft.Azure.SignalR.Connections.Client.Internal.WebSocketsTransport[6]
2022-11-11T15:10:37.632038589Z       Transport is stopping.
2022-11-11T15:10:37.634772560Z fail: Microsoft.Azure.SignalR.ServiceConnection[2]
2022-11-11T15:10:37.634793760Z       Failed to connect to '(Primary)https://clabsazes221111.service.signalr.net(hub=ChatSampleHub)', will retry after the back off period. Error detail: ManagedIdentityCredential authentication unavailable. No Managed Identity endpoint found.. ManagedIdentityCredential authentication unavailable. No Managed Identity endpoint found.. Id: fdc941cd-4e53-4139-96df-6a1e21f8b80f
```

> App Service apps are not created with a Managed Identity by default.

Configure the web app to use a system-generated Managed Identity:

```
az webapp identity assign -g labs-signalr -n <app-name>
```

Try it again, and you'll see it still doesn't work :) The logs should make it clear why.

> The web app can _authenticate_ to SignalR now with the Managed Identity, but the identity isn't _authorized_ to use the service.

The identity needs to be authorized with a [role assignment](https://learn.microsoft.com/en-gb/azure/azure-signalr/signalr-howto-authorize-managed-identity).

Get the ID of the SignalR Service - this is the _scope_ for the role:

```
az signalr show -g labs-signalr --query id -n <signalr-name>
```

Now create a role assignment for the App Service's Managed Identity ID, giving it the `SignalR App Server` role with the service scope:

```
# get the app service principal ID:
az webapp identity show --query principalId -o tsv -g labs-signalr -n <app-name>

# create the role assignment:
az role assignment create  --role 'SignalR App Server' --assignee-object-id <principalId> --scope "<signalr-id>"
```

📋 Check the SignalR Service in the Portal. Can you see the role assignment?

<details>
  <summary>Not sure how?</summary>
  
Role assignments are a generic authorization system in Azure. In the Portal:

- open _Access control (IAM)_
- browse to _Roles_ then select _SignalR App Server_ and _View_
- under _Assignments_ you should see the Web App

</details><br/>

The role assignment can take a few minutes to propagate, but the app will start working without needing a restart.

## Lab

There's not much more to SignalR - but it does have a very handy debugging tool. Configure tracing for messages in the Portal and open the trace tool. Open the chat site in some more browser windows and send messages. What does the trace tell you? Could you hack the site to broadcast messages to all users if you had the SignalR connection details?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources:

```
az group delete -y --no-wait n labs-signalr
```