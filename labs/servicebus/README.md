# Service Bus Messaging

Service bus - high throughput, reliable async message queue as a service.

## Reference

- [Service bus overview](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview)

- [Microsoft Learn: Implement message-based communication workflows with Azure Service Bus](https://learn.microsoft.com/en-us/training/modules/implement-message-workflows-with-service-bus/)

- [`az servicebus` commands](https://learn.microsoft.com/en-us/cli/azure/servicebus?view=azure-cli-latest)

## Create a Service Bus Namespace & Queue

Portal - 'service bus'; create:

- namespace name - unique subdomain at `.servicebus.windows.net`
- pricing tier - by max messag size, operation count or by MU


Create a namespace with the CLI:

```
az group create -n labs-servicebus -l westeurope --tags courselabs=azure

az servicebus namespace create --help

az servicebus namespace create -g labs-servicebus --location westeurope --sku Basic -n labsservicebuses # <unique-dns-name>
```

> output includes service bus endpoint - comms is over HTTPS

Open in the portal - you'll see the usual blades, plus queus and shared access. SAS policies are used for authentication and authorization - same concept as with storage accounts.

Communication is via queues - create one to send and listen for messages:

```
az servicebus queue create -g labs-servicebus --name echo --namespace-name labsservicebuses # <unique-dns-name>
```

Open the queue in the portal, you can see metrics on current usage.

## Run a .NET Subscriber

Subscribers listen on a queue in an infinite loop; when they receive a message they process it.

> You can run this locally if you have .NET 6 installed on your machine, otherwise run it from the Azure Cloud Shell

```
# get the connection string for your queue:
az servicebus namespace authorization-rule keys list -n RootManageSharedAccessKey -g labs-servicebus  --query primaryConnectionString -o tsv --namespace-name labsservicebuses # <unique-dns-name>

cd src/queues/queue-subscriber

dotnet run -cs '<connection-string>'
```

This app connects to the queue - it will sit and listen for messages.

In a different console run another app to publish messages:

```
cd src/queues/queue-publisher

dotnet run -cs '<connection-string>'
```


You'll see the publisher send messages and the subscriber receive them. In the portal you can see the metrics, but there aren't many message being processed at the moment.

## Reliable & Scalable Messaging

Check the number of the last batch processed by the subscriber, then end the subscriber (Ctrl-C or Cmd-C in the subscriber window.)

The publisher keeps running. Wait for it to publish some more batches.

If you run the subscriber again, it might do one of three things:

- process all the messages from batch 1 onwards
- only process new messages from when it starts listening
- process all the batches from when you stopped the subscriber

Run the subscriber. What does it do?

> Service bus queues store messages until they receive a completion acknowledgement. 

Open another console and run another instance of the subscriber:

```
cd src/queues/queue-subscriber

dotnet run -cs "$(az servicebus namespace authorization-rule keys list -n RootManageSharedAccessKey -g labs-servicebus  --query primaryConnectionString -o tsv --namespace-name labsservicebuses)"
```

Now the subscribers share the messages - there's no duplication, so instances can process their own set of messages.

## Lab

What happens with multiple publishers? You can run the subscriber without acknowledging - what happens to the messages?