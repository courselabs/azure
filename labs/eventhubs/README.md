# Event Hubs

Large quantities of data - multiple producers, multiple consumers (IoT).


## Reference

- [Event Hubs overview](https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-about)

- [Scalability and Throughput Units](https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-scalability)

- [`az eventhubs` commands](https://learn.microsoft.com/en-us/cli/azure/eventhubs?view=azure-cli-latest)

## Create an Event Hub Namespace & Hub

Portal - 'event hub'; create:

- namespace name - same concept as service bus `.servicebus.windows.net`
- pricing tier - ingress events per million
- Throughput Units - messaging capacity & scale


Create an RG and an Event Hub namespace:

```
az group create -n labs-eventhubs -l westeurope --tags courselabs=azure

az eventhubs namespace create --help

# create a namespace with set TLS and capacity:
az eventhubs namespace create --min-tls 1.2 --capacity 2 --location westeurope --sku Basic -g labs-eventhubs -n labseventhubses # <unique-dns-name>
```

Open the namespace in the Portal:

- what does the capacity in the command refer to?
- how do you authenticate to send and receive events?

Browse to the Event Hubs for the namespace and look at the options for creating a new one:

- partitions are a key concept in Event Hubs - they directly affect scalability
- message retention can be configured up to 90 days on the Premium tier

## Create an Event Hub & Publish Events

The namespace is a grouping mechanism, you need to create an actual Event Hub to send and receive:

```
az eventhubs eventhub create --help

# create an Event Hub with set partitions & retention:
az eventhubs eventhub create --name devicelogs --partition-count 3 --message-retention 1 -g labs-eventhubs --namespace-name labseventhubses # <unique-dns-name>
```

Open the new Event Hub in the Portal. You will see options to Capture and Process data - Event Hubs can use custom code to process events, or they can be automatically ingested into Azure storage services.

Event Hub client code is similar to Service Bus publisher:

- src/eventhubs/publisher/Program.cs

Connection uses the same access policy semantics, and Namespaces are created with the same default admin policy name:

```
# get the connection string:
az eventhubs namespace authorization-rule keys list -n RootManageSharedAccessKey --query primaryConnectionString -o tsv -g labs-eventhubs --namespace-name labseventhubses # <unique-dns-name>

# run the publisher:
dotnet run --project ./src/eventhubs/producer -cs '<connection-string>'
```

You'll see 20 different producers, each send a batch of 100 messages.

Check in te Portal to see how the traffic is shown.


## Preview the Events with Stream Analytics

Managed data processing - open the _Process Data_ tab:

- under _Process your Event Hub data using Stream Analytics Query Language._ click _Start_
- a preview window opens with a simpl SQL-like query
- events will load into the preview window

Run the publisher again to see more events. The preview table shows all the fields from the publishd events, plus some fields added by Event Hubs (EventProcessedTime, PartitionId etc).

> Stream Analytics lets you collect, filter and store events without writing code

Make a note of the earliest event time. Exit the query editor and then load it again - do the same events get shown?

> Yes. The data is retained in the Event Hub until it expires; the consumer can choose how far back to read


## Receive Events from a Consumer Group

