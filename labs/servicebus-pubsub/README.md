# Service Bus Publish-Subscribe

Messaging patterns - pub-sub, multiple subscribers - for async service consumption & extensibility.

## Reference


- [`az servicebus topic` commands](https://learn.microsoft.com/en-us/cli/azure/servicebus/topic?view=azure-cli-latest)


## Create a Service Bus Namespace & Topic

Create a namespace with the CLI:

```
az group create -n labs-servicebus-pubsub -l westeurope --tags courselabs=azure

# create with TLS 1.2 and Standard tier - needed for topics
az servicebus namespace create -g labs-servicebus-pubsub --location westeurope --sku Standard --min-tls 1.2 -n labsservicebuspubsubes2 # <unique-dns-name>
```

Create a topic:

```
# check the options:
az servicebus topic create --help

# create a topic with a set max message size and TTL:
az servicebus topic create --max-size 2048 --default-message-time-to-live P0DT0H10M1S -n broadcast -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name>
```

Create a queue for comparison:

```
az servicebus queue create --max-size 1024 --default-message-time-to-live P0DT0H1M0S -n command -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name>
```

Open in Portal - what do you see with a topic that you don't see with a queue?

> Topics have subscriptions

## Create Subscriptions

Subscriptions are like channels for routing. Publishers send messages to the topic as a whole, and all the subscriptions receive a copy of the message. You typically have multiple subscriptions, each with one subscriber listening and processing messages.

Create a subscription for the topic:

```
# print the optionds:
az servicebus topic subscription create --help

# create two subscriptions: 
az servicebus topic subscription create --name web --topic-name broadcast -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 

az servicebus topic subscription create --name desktop --topic-name broadcast -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 
```

Printing the details of a subscription includes how many messages are there:

```
az servicebus topic subscription show --name detail --topic-name broadcast -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 

# add a query to print message count:
az servicebus topic subscription show --name web --topic-name broadcast --query messageCount -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 
```

## Publish Messages to the Topic

src/servicebus/publisher/Program.cs  cs code - same pattern, only the name of the topic is needed, not the subscriber

Usually you have more fine-grained permissions than the default. Create a sender role which only has permissions for the topic:

```
az servicebus topic authorization-rule create --help

az servicebus topic authorization-rule create --topic-name broadcast --name publisher --rights Send -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 
```

Get the connection string for that role and use it for the publisher app:

```
az servicebus topic authorization-rule keys list --topic-name broadcast --name publisher --query primaryConnectionString -o tsv -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 

cd src/servicebus/publisher

dotnet run -topic broadcast -cs '<publisher-connection-string>'

# after some batches have been sent, exit the app 
# ctrl-c or cmd-c
```

Check both subscriptions have the same message count:

```
az servicebus topic subscription show --name web --topic-name broadcast --query messageCount -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 

az servicebus topic subscription show --name desktop --topic-name broadcast --query messageCount -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 
```


## Receive Messages from a Subscription

Create an authorization rule with Listen permission on the topic:

```
# create the rule:
az servicebus topic authorization-rule create --topic-name broadcast --name subscriber --rights Listen -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 

# print the connection string:
az servicebus topic authorization-rule keys list --topic-name broadcast --name subscriber --query primaryConnectionString -o tsv -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 
```

```
cd ../subscriber

dotnet run -topic broadcast -subscription web -cs '<subscriber-connection-string>'
```

> Do you see the messages? The topic has a default expiry time of 10 minutes, so if that time has elapsed then the subscriber won't get any messages

You can use the Service Bus Explorer in the Portal to see all the messages for the subscription.


Leave the subscriber running and start the publisher again in another console:

```
cd src/servicebus/publisher

dotnet run -topic broadcast -cs '<publisher-connection-string>'
```

You should see the subscriber printing all the messages.


## Receive Messages from Both Subscriptions

You can have as many subscriptions as you need to model your application. 

One subscriber is consuming messages from the `web` subscription, but they are not being consumed by anything for the `desktop` subscription. 

Check there are messages waiting:

```
az servicebus topic subscription show --name desktop --topic-name broadcast --query messageCount -g labs-servicebus-pubsub  --namespace-name labsservicebuspubsubes2 # <unique-dns-name> 
```

Start a subscriber for the `desktop` subscription in a new console:

```
cd src/servicebus/subscriber

dotnet run -topic broadcast -subscription desktop -cs '<subscriber-connection-string>'
```

The new subscriber gets a copy of all the un-expired messages; when it has received all the old messages it will receive new ones at the same time as the `web` subscriber.


## Lab

What happens with multiple subscribers listening on the same subscription?

## Cleanup

```
az group delete --query "[?tags.courselabs=='azure']"
```