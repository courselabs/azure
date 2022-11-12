# Service Bus Publish-Subscribe

One of the main patterns in asynchronous messaging is _publish-subscribe_ (pub-sub). The component sending messages is the publisher, and there can be zero or many components who subscribe to the message - and they all get a copy. This is great for extensible architectures, new subscribers can be added with new functionality, without any changes to existing components.

In this lab we'll use Service Bus _topics_ for pub-sub messaging, and see what happens when we add subscribers to a topic.

## Reference

- [The pub-sub pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/publisher-subscriber)

- [Service Bus queues, topics and subscriptions](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-queues-topics-subscriptions)

- [`az servicebus topic` commands](https://learn.microsoft.com/en-us/cli/azure/servicebus/topic?view=azure-cli-latest)

- [`az servicebus topic subscription` commands](https://learn.microsoft.com/en-us/cli/azure/servicebus/topic/subscription?view=azure-cli-latest)

## Create a Service Bus Namespace & Topic

We'll start with a Service Bus Namespace (we covered this in the [Service Bus]() lab), but we need at least Standard tier to get the topics feature:

```
az group create -n labs-servicebus-pubsub  --tags courselabs=azure -l westeurope

# create with TLS 1.2 and Standard tier - needed for topics
az servicebus namespace create -g labs-servicebus-pubsub --sku Standard --min-tls 1.2 -l westeurope -n <sb-name> 
```

Open the namepace in the Portal. Namespaces are the container for multiple queues and topics. Click to create a topic - there are a couple of interesting options:

- _time to live_ (TTL) - defines how long messages stay available if there are no subscribers to pick them up
- maximum topic size - topics store messages and forward them to subscribers, so you can set the maxmimum amount of storage

📋 Create a topic called `broadcast` with a `servicebus topic` command, specifying a TTL of 10 minutes and a maximum size of 2GB.

<details>
  <summary>Not sure how?</summary>

Check the help text:

```
az servicebus topic create --help
```

You can set TTL using a [duration format]() which sets the number of datys, hours, minutes and seconds:

```
az servicebus topic create --max-size 2048 --default-message-time-to-live P0DT0H10M1S -n broadcast -g labs-servicebus-pubsub  --namespace-name <sb-name> 
```

</details><br/>

Create a queue for comparison - you can also set TTL and maximum size for queues:

```
az servicebus queue create --max-size 1024 --default-message-time-to-live P0DT0H1M0S -n command -g labs-servicebus-pubsub  --namespace-name <sb-name> 
```

Compare the two in Portal - they're both destinations where a publisher can send messages. What do you see with a topic that you don't see with a queue?

> Topics have _subscriptions_. You can't listen on a topic like you can with a queue, consumsers need to have a subscription to listen on.

## Create Subscriptions

Subscriptions are like channels for routing. Publishers send messages to the topic as a whole, and all the subscriptions receive a copy of the message. You typically have multiple subscriptions, each with one component listening and processing messages.

In a store application you may have a component which publishes an `order-created` message to the topic, with multiple subscriptions used for different features:

- the fulfilment component processes the shipping request
- an analytics component summarizes the data
- an auditing component traces the details of the order

📋 Create two subscriptions for the topic, one called `web` and one called `desktop`.

<details>
  <summary>Not sure how?</summary>

Subscriptions have their own commands under the topic group:

```
az servicebus topic subscription create --help
```

Subscriptions need to be created for a specific topic, then they just need a name:

```
az servicebus topic subscription create --name web --topic-name broadcast -g labs-servicebus-pubsub  --namespace-name <sb-name>  

az servicebus topic subscription create --name desktop --topic-name broadcast -g labs-servicebus-pubsub  --namespace-name <sb-name>  
```

</details><br/>

Printing the details of a subscription includes how many messages are there:

```
az servicebus topic subscription show --name desktop --topic-name broadcast -g labs-servicebus-pubsub  --namespace-name <sb-name>  
```

You can query just the message count - _this is the number of messages available to be read in one topic:_

```
az servicebus topic subscription show --name web --topic-name broadcast --query messageCount -g labs-servicebus-pubsub  --namespace-name <sb-name>  
```

No messages so far.

## Publish Messages to the Topic

We have a simple .NET 6 app which publishes messages to the topic:

- [publisher/Program.cs](/src/servicebus/publisher/Program.cs) - uses exactly the same code to send to a queue or a topic, from the sender's point of view it doesn't matter which it is

Our application will publish to the topic so we need an access policy. Every namespace has a root policy with permission to everything, but we should be careful not to use any more permissions than we need.

Create a new authorization rule for a sender role which only has permissions to send messages to this topic:

```
az servicebus topic authorization-rule create --help

az servicebus topic authorization-rule create --topic-name broadcast --name publisher --rights Send -g labs-servicebus-pubsub --namespace-name <sb-name>  
```

Now we can get the connection string for that role and use it for the publisher app (you'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download) to run the app locally):

```
# get the connection string for the sender role:
az servicebus topic authorization-rule keys list --topic-name broadcast --name publisher --query primaryConnectionString -o tsv -g labs-servicebus-pubsub  --namespace-name <sb-name>  

# run the app - make sure to 'quote' the connection string:
dotnet run --project src/servicebus/publisher -topic broadcast -cs '<publisher-connection-string>'

# wait for the app to send a few batches, then exit
# ctrl-c or cmd-c
```

📋 Check both subscriptions have the same message count.

<details>
  <summary>Not sure how?</summary>

```
az servicebus topic subscription show --name web --topic-name broadcast --query messageCount -g labs-servicebus-pubsub  --namespace-name <sb-name>  

az servicebus topic subscription show --name desktop --topic-name broadcast --query messageCount -g labs-servicebus-pubsub  --namespace-name <sb-name>  
```

</details><br/>

Both subscriptions will have the same count, the topic forwards all messages to all subscriptions.

Check in the Portal and you can use _Service Bus Explorer_ in the subscription blade to inspect the messages.

## Receive Messages from a Subscription

Access policies can be applied to the namespace as a whole, or to individual queues or topics. Subscriptions aren't indepdently secured, so we'll create an access policy which will give read access to any subscription in the topic:

```
az servicebus topic authorization-rule create --topic-name broadcast --name subscriber --rights Listen -g labs-servicebus-pubsub  --namespace-name <sb-name>  
```

The application code which reads messages is in a separate program from the publisher:

- [subscriber/Program.cs](/src/servicebus/subscriber/Program.cs) - the logic is the same for queues and topics, the only difference is that processor needs to be initialized with the topic name and the subscription name

```
# print the connection string:
az servicebus topic authorization-rule keys list --topic-name broadcast --name subscriber --query primaryConnectionString -o tsv -g labs-servicebus-pubsub  --namespace-name <sb-name>  

# run a subscriper on the web subscription:
dotnet run --project src/servicebus/subscriber  -topic broadcast -subscription web -cs '<subscriber-connection-string>'
```

> Do you see the messages? The topic has a default expiry time of 10 minutes, so if that time has elapsed then the subscriber won't get any messages

Leave the subscriber running and start the publisher again in another console:

```
dotnet run --project src/servicebus/publisher -topic broadcast -cs '<publisher-connection-string>'
```

You should see the publisher logging when it sends a batch, and the subscriber printing all the messages it receives.

## Receive Messages from Both Subscriptions

You can have as many subscriptions as you need to model your application. 

One subscriber is consuming messages from the `web` subscription, but the `desktop` subscription doesn't have any consumers.

Compare the message counts in the subscriptions again:

```
az servicebus topic subscription show --name web --topic-name broadcast --query messageCount -g labs-servicebus-pubsub  --namespace-name <sb-name>  

az servicebus topic subscription show --name desktop --topic-name broadcast --query messageCount -g labs-servicebus-pubsub  --namespace-name <sb-name>  
```

> You should see the `web` subscription has 0 messages, because they've all been delivered to your consumer. The `desktop` subscription will have a copy of every message that's been published and not yet expired.

Start a subscriber for the `desktop` subscription in a new console:

```
dotnet run --project src/servicebus/subscriber -topic broadcast -subscription desktop -cs '<subscriber-connection-string>'
```

The new subscriber gets a copy of all the un-expired messages, so it will print lots of logs while the `web` subscriber is waiting for messages. When it has received all the old messages, it has caught up on the backlog and it will wait - receiving new messages at the same time as the `web` subscriber.


## Lab

Service Bus is a reliable and scalable messaging solution. You use subscriptions to model different processes which could operate at different speeds. But you don't need to have a single consumer in each subscription.

What happens with multiple subscribers listening on the same subscription? And with multiple publishers publishing to the same topic?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y --no-wait -n labs-servicebus-pubsub
```
