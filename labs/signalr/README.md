# Azure SignalR Service

websockets backend for asyc comms - broadcast updates to web pages

## Reference

- [Azure SignalR Service overview](https://learn.microsoft.com/en-us/azure/azure-signalr/signalr-overview)

- [`az signalr` commands](https://learn.microsoft.com/en-us/cli/azure/signalr?view=azure-cli-latest)

## Run a Local SignalR Website

```
dotnet run --project src/signalr/chat --urls=http://localhost:5005/
```

Open two browser windows to http://localhost:5005/

Exchange messages :) Now end the server app with Ctrl-C - what happens on the browser?

Restart the app with the same command:

```
dotnet run --project src/signalr/chat --urls=http://localhost:5005/
```

You can reconnect the browser with F5, but the previous messages are lost.

What if you run at scale? You can run another copy of the website using a different port, but it's a completely separate instance:


```
dotnet run --project src/signalr/chat --urls=http://localhost:5006/
```

Messages aren't shared between the two servers, so users see a different set of messages depending on which server they're connected to.

## Create SignalR Service

Portal, new resource search for SIgnaR - select _SignalR Service_ & create:

- DNS name, region, RG
- pricing tier defaults to Premium - also free & standard (scale & reliability)
- follow the links to find out about _Units_ and _Service Mode_

> SignalR is one of the less well-documented services :)

No Vnet integration.

Create with CLI:

```
az group create -n labs-signalr --tags courselabs=azure 

az signalr create -g labs-signalr --sku Free_F1 -n <signalr-name>
```

> YOu may see a message _Resource provider 'Microsoft.SignalRService' used by this operation is not registered. We are registering for you._

SignalR is one of the less common services, so it probably won't be enabled in your subscription by default. The CLI will set it up for you.

Browse to the RG in the Portal:

- keys
- connection strings

What authentication methods are available for the app?

Grab the _Connection string for Access Key_ and use it with your local app. Stop all other instances then:

Endpoint=https://clabsazes221111.service.signalr.net;AccessKey=+jNfcv1dc+Oymcsme8Y0mXvgUp1Li8U8V635vo0MBFs=;Version=1.0;

```
--Database:Api=Mongo --ConnectionStrings:AssetsDb='<cosmos-connection-string>'

dotnet run --project src/signalr/chat --urls=http://localhost:5005/ --Azure:SignalR:Enabled=true --Azure:SignalR:ConnectionString='Endpoint=https://clabsazes221111.service.signalr.net;AccessKey=+jNfcv1dc+Oymcsme8Y0mXvgUp1Li8U8V635vo0MBFs=;Version=1.0;'

dotnet run --project src/signalr/chat --urls=http://localhost:5006/ --Azure:SignalR:Enabled=true --Azure:SignalR:ConnectionString='Endpoint=https://clabsazes221111.service.signalr.net;AccessKey=+jNfcv1dc+Oymcsme8Y0mXvgUp1Li8U8V635vo0MBFs=;Version=1.0;'
```

Now open one browser to each server:

- http://localhost:5005/ 
- http://localhost:5006/

Can you exchange messages this time? What happens if you stop one server and restart - are the messages preserved this time?

> SignalR is for real-time broadcasting; if you want your app to maintain state then you need to take care of that

## Deploy SignalR Website to Azure

The SignalR service supports authentication with Managed Identities. We can run the website in an App Service with a System-Managed Identity and connect to SignalR without needing sensitive data in the connection string.

```
cd src/signalr/chat

az webapp up -g labs-signalr --os-type Linux --sku B1 --runtime dotnetcore:6.0 -n <app-name>
```

Set the connection string details - for MI you just need the SignalR domain name:

```
az webapp config appsettings set --settings Azure__SignalR__Enabled='true' Azure__SignalR__ConnectionString='Endpoint=https://<signalr-name>.service.signalr.net;AuthType=azure.msi;Version=1.0;' -g labs-signalr -n <app-name>
```

> Browse to the app. It won't work - can you see the reason in the logs?

Page loads but message send fails. Check the _Log stream_ in the Web App:

```
2022-11-11T15:10:37.631828591Z info: Microsoft.Azure.SignalR.Connections.Client.Internal.WebSocketsTransport[6]
2022-11-11T15:10:37.632038589Z       Transport is stopping.
2022-11-11T15:10:37.634772560Z fail: Microsoft.Azure.SignalR.ServiceConnection[2]
2022-11-11T15:10:37.634793760Z       Failed to connect to '(Primary)https://clabsazes221111.service.signalr.net(hub=ChatSampleHub)', will retry after the back off period. Error detail: ManagedIdentityCredential authentication unavailable. No Managed Identity endpoint found.. ManagedIdentityCredential authentication unavailable. No Managed Identity endpoint found.. Id: fdc941cd-4e53-4139-96df-6a1e21f8b80f
```


Configure the web app with a system MI to get it working.

```
az webapp identity assign -g labs-signalr -n <app-name>
```

> try it again. Still not working 

The identity needs to be authorized to use SignalR with a [role assignment](https://learn.microsoft.com/en-gb/azure/azure-signalr/signalr-howto-authorize-managed-identity):

Get the ID of the SignalR - this is the scope for the role:

```
az signalr show -g labs-signalr --query id -n <signalr-name>
```

```
az role assignment create  --role 'SignalR App Server' --assignee-object-id <principalId> --scope "<signalr-id>"
```

Check the SignalR in the Portal. Can you see the role assignment?

- _Access control (IAM)_
- _Roles_ then select _SignalR App Server_
- _Assignments_ - should see the Web App in there

Can take a while to propagate, but the app will start working without needing a restart.

## Lab

There's not much more to SignalR - but it does have a very handy debugging tool. Configure tracing for messages in the Portal and open the trace tool. Open the chat site in some more browser windows and send messages. What does the trace tell you? Could you hack the site to broadcast messages to all users if you had the SignalR connection details?