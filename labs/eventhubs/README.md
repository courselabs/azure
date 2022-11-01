# Event Hubs

Event Hub is a messaging service aimed at large scale, where you have lots of producers publishing events - think millions per day or billions per month. An event is practically the same as a message, but the intention is different. A message might be a command asking for something to be done, or a request expecting a response. An event is just a record that something has happened, and consumers decide which types of event to listen for and what to do when they happen.

In this lab we'll start using Event Hubs and see what it's like to work with data coming in as stream of events.

## Reference

- [Event Hubs overview](https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-about)

- [Scalability and Throughput Units](https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-scalability)

- [`az eventhubs` commands](https://learn.microsoft.com/en-us/cli/azure/eventhubs?view=azure-cli-latest)

## Create an Event Hub Namespace & Hub

Create a new resource in the Portal and search for 'event hub'. Click to create and explore the options:

- hubs have a namespace name - which is same concept as service bus `.servicebus.windows.net`
- pricing tier is comsumption based - ingress events per million
- Throughput Units - define messaging capacity & scale

_Ingress_ refers to messages coming in to Event Hubs from the producers.

Switch to the CLI to create an RG and an Event Hub namespace:

```
az group create -n labs-eventhubs --tags courselabs=azure -l westeurope

# create a namespace with set TLS and capacity:
az eventhubs namespace create --min-tls 1.2 --capacity 2  --sku Basic -g labs-eventhubs -l westeurope -n <eh-name> 
```

Open the namespace in the Portal and browse to your Event Hub Namespace:

- what does the capacity in the command refer to?
- how do you authenticate to send and receive events?

Open the _Event Hubs_ tab for the namespace and look at the options for creating a new one:

- `partitions` are a key concept in Event Hubs - they directly affect scalability
- message retention can be configured up to 90 days on the Premium tier

_Partitions_ spit the incoming message stream, which gives you greater capacity to run at scale. More partitions means you can have more concurrency, greater numbers of producers and consumers all running at the same time and working with the same hub.

## Create an Event Hub & Publish Events

The namespace is a grouping mechanism, you need to create an actual Event Hub to send and receive.

📋 Create an event hub called `devicelogs` with 3 partitions, and a retention period of one day.

<details>
  <summary>Not sure how?</summary>

Check the help:

```
az eventhubs eventhub create --help
```

Create an Event Hub with set partitions & retention:

```
az eventhubs eventhub create --name devicelogs --partition-count 3 --message-retention 1 -g labs-eventhubs --namespace-name <eh-name>
```

</details><br/>

Open the new Event Hub in the Portal. You will see options to Capture and Process data - Event Hubs can use custom code to process events, or they can be automatically ingested into Azure data and analytics services.

Working with Event Hubs in code is similar to using Service Bus. This is simple app which publishes events:

- [producer/Program.cs](/src/eventhubs/producer/Program.cs) - events are always sent in a batch, and the event content itself is just a byte array; this code serializes an object to JSON and the library produces bytes from the JSON string

Event Hub connections uses the same access policy concept as Service Bus, and Namespaces are created with the same default admin policy name `RootManageSharedAccessKey`.

📋 Print the connection string to use for the root authorization rule.

<details>
  <summary>Not sure how?</summary>

```
az eventhubs namespace authorization-rule keys list -n RootManageSharedAccessKey --query primaryConnectionString -o tsv -g labs-eventhubs --namespace-name <eh-name>
```

</details><br/>

Run the producer locally (you'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download)), publishing messages to your Event Hub:

```
# be sure to 'quote' the connection string:
dotnet run --project src/eventhubs/producer -cs '<connection-string>'
```

You'll see 10 different producers each send a batch of 10 messages, then the program will exit.

Check in the Portal to see how the traffic is shown.

## Preview the Events with Stream Analytics

Event Hubs come with managed data processing options, so for standard scenarios you might not need to write any consumer code. Open the _Process Data_ tab:

- under _Process your Event Hub data using Stream Analytics Query Language_ click _Start_
- a preview window opens with a simpl SQL-like query
- click the _Create_ button to set up the connection from the query to the Event Hub
- events will load into the preview window

Run the publisher again and refresh the query to see more events - they make take a moment to come through. The preview table shows all the fields from the published events, plus some fields added by Event Hubs (EventProcessedTime, PartitionId etc).

> Stream Analytics lets you collect, filter and store events without writing code

Make a note of the earliest event time. Exit the query editor and then load it again - do the same events get shown?

> Yes. The data is retained in the Event Hub until it expires; the consumer can choose how far back to read

## Receive Events from a Consumer Group

Event Hubs may look a bit like Service Bus Queues or Topics, but they work in a very different way. Service Bus keeps track of which messages have been processed, removing them when a consumer flags that they've been completed. Event Hubs retains all messages until they expire, there' no notion of removing an event from the hub.

Instead it's the consumer's responsibility to record the events it has processed, so if it gets stopped and restarted it needs to know where to pick up from. That gives you the infrastructure to build a highly reliable processing system where events are never missed, even when they're produced at massive scale.

But it's a bit complex so we'll start with something simpler:

- [consumer/Program.cs](/src/eventhubs/consumer/Program.cs) - connects to the Event Hub, reads 50 events, prints some details and then exits

This application would struggle to cope with high scale, but it's fine for our example app. You can run it with the same connection string as the producer:

```
# run the consumer:
dotnet run --project ./src/eventhubs/consumer -cs '<connection-string>'
```

> You may see events being read from different partitions. This is a simple consumer which doesn't have any logic around the events it receives.

Run the consumer a few more times and you will see events from different partitions; keep running it and you'll see the same events being read repeatedly:

```
dotnet run --project ./src/eventhubs/consumer -cs '<connection-string>'
```

The consumer code is pulling 50 events more-or-less at random from the event stream. It doesn't keep a record of what it's done so far, so each time it runs it will pull 50 events at random again, and that could include some it has already processed.

## Lab

The consumer prints some more information about the message - including the _partition_ and the _offset_. Those two pieces of data enable reliable processing at scale, ensuring events get processed at least once. How could you use that data to build multiple consumers which can run in parallel and don't keep getting the same events if they restart?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y --no-wait -n labs-eventhubs
```
