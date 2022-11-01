# Service Bus Messaging

Service Bus is a high throughput, reliable message queue serivce. Messages can be stored until read, and there are advanced features like a dead-message queue for messages which were never delivered, or failed processing. You can use Service Bus queues to support the standard messaging patterns.

In this lab we'll use a fire-and-forget messaging pattern, where a publisher sends messages without expecting a return, or even knowing which component will process them.

## Reference

- [Service bus overview](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview)

- [Microsoft Learn: Implement message-based communication workflows with Azure Service Bus](https://learn.microsoft.com/en-us/training/modules/implement-message-workflows-with-service-bus/)

- [`az servicebus` commands](https://learn.microsoft.com/en-us/cli/azure/servicebus?view=azure-cli-latest)

## Create a Service Bus Namespace & Queue

The Service Bus resource you create is a _namespace_, which is a grouping construct for multiple queues. Create a new resource in the Portal and search for 'service bus'. Click to create a _Service Bus_ resource - this is actually the namespace. Explore the options:

- the namespace name will give you a unique subdomain at `.servicebus.windows.net`
- pricing tiers define the maximum message size, features and operation count
- you can set the minimum TLS level for consumers

Create a namespace with the CLI:

We'll switch to the CLI now. Create a Resource Group for the lab:

```
az group create -n labs-servicebus --tags courselabs=azure -l westeurope
```

📋 Create a namespace with a `servicebus namespace` command, using the Basic SKU.

<details>
  <summary>Not sure how?</summary>

Check the help text:

```
az servicebus namespace create --help
```

Only name and RG are required, but the default SKU is Standard so we need to set that:

```
az servicebus namespace create -g labs-servicebus --location westeurope --sku Basic -n <sb-name>
```

</details><br/>

> The output includes service bus endpoint - comunication is over HTTPS

Open the Service Bus Namespace in the portal - you'll see the usual blades, plus _Queues_ and _Shared access policies_. Shared access tokens are used for authentication and authorization, similar to storage accounts, except there's a one-to-one relationship between policies and tokens.

In the Basic SKU the only messaging option is a queue - create one to send and listen for messages:

```
az servicebus queue create -g labs-servicebus --name echo --namespace-name <sb-name>
```

> Open the queue in the portal and you see metrics on message counts.

There's also a _Shared access policies_ tab at the queue level, so you can create fine-grained permissions for apps which need to send to one queue and read from another.

## Run a .NET Subscriber

Subscribers listen on a queue in an infinite loop; when they receive a message they process it. In a distributed application you may have multiple components which each subscribe to different queues, and each component could have multiple instances. 

Service Bus uses a standard protocol called Advanced Message Queuing Protocol [AMQP](http://docs.oasis-open.org/amqp/core/v1.0/amqp-core-overview-v1.0.html). Other queue technologies implement the same protocol, so Service Bus can be used as a drop-in replacement for RabbitMQ and others. 

We have a simple app which subscribes to the queue using the Service Bus client library:

- [subscriber/Program.cs](/src/servicebus/subscriber/Program.cs) - subscribes to the queue and prints the contents of every message it receives, then it acknowledges the message has been processed

Run the app locally (you'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download)), using a parameter to set the connection string:

```
# get the connection string for your queue:
az servicebus namespace authorization-rule keys list -n RootManageSharedAccessKey -g labs-servicebus  --query primaryConnectionString -o tsv --namespace-name <sb-name>

# run the app:
dotnet run --project src/servicebus/subscriber -cs '<connection-string>'
```

The app will listen for messages until you shut it down.

We have another simple app which publishes messages to the queue in a loop - publishing a batch, waiting and then publishing another batch:

- [publisher/Program.cs](/src/servicebus/publisher/Program.cs) - sends a batch of messages, which is typically more efficient than making separate connections to send individual messages

In a different console run the publisher app:

```
dotnet run --project src/servicebus/publisher -cs '<connection-string>'
```

You'll see the publisher send messages and the subscriber receive them. In the portal you can see the metrics, but there aren't many message being processed at the moment.

## Reliable & Scalable Messaging

Check the number of the last batch processed by the subscriber, then end the **subscriber** (Ctrl-C or Cmd-C in the subscriber window) - leave the publisher app running.

The publisher keeps send messages. Wait for it to publish some more batches.

If you run the subscriber again, it might do one of three things:

- process all the messages from batch 1 onwards
- only process new messages from when it starts listening
- process all the batches from when you stopped the subscriber

📋 Run the subscriber. What does it actually do?

<details>
  <summary>Not sure how?</summary>

It's the same command:

```
dotnet run --project src/servicebus/subscriber -cs '<connection-string>'
```

You should see the subscriber pick up where it left off, processing the new batches that were published since you closed the previous instance of the subscriber.

</details>

> Service bus queues store messages until they receive a completion acknowledgement.

New subscribers don't get any messages which have been flagged as completed, but they do get all the un-completed messages in the queue. That way requests don't get lost or processed twice if a subscriber fails.

Open another console and run another instance of the subscriber:

```
dotnet run --project src/servicebus/subscriber -cs '<connection-string>'
```

Now the subscribers share the messages - they take turns to receive a message (more-or-less). There's no duplication, so instances can process their own set of messages.

## Lab

Messaging is all about distributing work at scale. What happens when there are multiple publishers - how do the subscribers split the work? 

Reliability is a key factor too. You can run the subscriber without acknowledging message completion using the `-ack False` flag. If you only have a single subcriber which doesn't acknowledge messages, and you quit and replace that subscriber, what happens to the messages it processed?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y --no-wait -n labs-servicebus 
```
