# Event Hubs Partitioned Consumers

Processing a partitioned stream of events reliably needs careful logic, which Microsoft have built into client libaries for various languages. The library takes care of recording the processed offset (using blob storage as a simple store for state), ensuring that each consumer picks up where it left off in the stream. It also supports running at scale with multiple consumers - and if one consumer fails, one of the others will pick up its work.

In this lab we'll see this _partitioned consumer_ pattern in progress, seeing how each instance records its progress and how we get high availability and scale.

## Reference

- [Balance Event Hubs partition load](https://learn.microsoft.com/en-us/azure/event-hubs/event-processor-balance-partition-load)

- [Competing consumer pattern](https://learn.microsoft.com/en-us/previous-versions/msp-n-p/dn568101(v=pandp.10))

- [`az eventhubs eventhub consumer-group` commands](https://learn.microsoft.com/en-us/cli/azure/eventhubs/eventhub/consumer-group?view=azure-cli-latest)

## Create an Event Hub Namespace & Storage Acccount

We'll start with the core resources - an RG and an Event Hub namespace. The Event Hub needs to be Standard SKU or higher to have support the partitioned consumer pattern:

```
az group create -n labs-eventhubs-consumers --tags courselabs=azure -l westeurope

az eventhubs namespace create --min-tls 1.2 --capacity 2 --sku Standard -g labs-eventhubs-consumers -l westeurope -n <eh-name> 
```

We'll also need a Storage Account and some blob containers which the consumers will use to store their progress. We'll have two different sets of processing, one needs a container to store the offsets and the other will store a copy of all events:

```
az storage account create --sku Standard_ZRS -g labs-eventhubs-consumers -l westeurope -n <eh-name>

az storage container create -n checkpoints  -g labs-eventhubs-consumers --account-name <eh-name>

az storage container create -n devicelogs  -g labs-eventhubs-consumers --account-name <eh-name>
```

> There's no direct link between the Event Hub and the Storage Account - they are only brought together in the consumer code.

Open the Event Hub namespace in the Portal - there's one extra option in the left menu: _Networking_. How might you use private networking with Event Hubs?

## Create an Event Hub & Consumer Groups

Partition count is set when the Event Hub is created and it can't be changed. You need to consider that carefully because more partitions cost more, but fewer partitions limit your ability to scale.

📋 Create an event hub called `devicelogs` with 5 partitions, and a retention period of two days.

<details>
  <summary>Not sure how?</summary>

```
# standard SKU allows for longer message retention:
az eventhubs eventhub create --name devicelogs --partition-count 5 --message-retention 2 -g labs-eventhubs-consumers --namespace-name <eh-name> 
```

</details><br/>

> More expensive SKUs let you set longer retention periods which is useful if your producers are bursty - you might find the consumers can't process all the events in good time during high periods, so the longer retention gives them more time to work through the peaks

Open the Event Hub in the Portal and check the _Consumer groups_ tab. There's a `$Default` consumer group in every Event Hub - can you delete it?

We can create multiple consumer groups in this SKU - you would use different groups for different features. They can run at different levels of scale (e.g. business processing with multiple consumers and an audit log with a single consumer):

```
# check the options - this is at the Event Hub level, not the namespace:
az eventhubs eventhub consumer-group create --help

# create a processing group:
az eventhubs eventhub consumer-group create -n processing --eventhub-name devicelogs -g labs-eventhubs-consumers --namespace-name <eh-name>

# and an auditing group
az eventhubs eventhub consumer-group create -n auditing --eventhub-name devicelogs -g labs-eventhubs-consumers --namespace-name <eh-name>
```

> Check in the Portal again - the two consumer groups are listed along with the default, but there is nothing you can do with them (other than delete them)

Consumer groups are a mechanism for isolating data reads. They're conceptually similar to Service Bus topics, allowing different components to get all the same data but process it at different speeds. In Event Hubs it's the consumer's responsibility to manage the reads.

## Publish Events and Capture

We'll use the same publisher app as before. It will need the connection string for the new Event Hub namespace:

```
# get the connection string:
az eventhubs namespace authorization-rule keys list -n RootManageSharedAccessKey --query primaryConnectionString -o tsv -g labs-eventhubs-consumers --namespace-name <eh-name>

# this will send 100 batches of 50 events:
dotnet run --project ./src/eventhubs/producer -ProducerCount 100 -BatchSize 50 -cs '<connection-string>'
```

The publishing semantics are the same whether there are multiple consumer groups or a single one (there's always a default consumer group in every Event Hub). If we were doing this for real we'd create a dedicated access policy with just Send permissions for this hub.

Standard SKU offers additional features, including the ability to store all events in blob storage. Open the Event Hub in the Portal and set up _Capture_:

- choose the Avro output format and select _On_ to enable capture
- slide the _Size window_ down to the minimum
- tick _Do not emit empty files when no events occur during the Capture time window_
- choose your `devicelogs` blob storage container

Leave the rest of the fields with their defaults, save the changes and the open the Storage Account. 

In the `devicelogs` container you should see your events stored as blobs (they may take a couple of minutes to come through - if you don't see any it could be that they were published too long ago for the capture to see them. Just run the publisher again). What does the folder structure signify? Open one of the Avro files, what does it contain?

> There's a sample in [13.avro](/labs/eventhubs-consumers/13.avro)

## Run Processing Consumers

We can use the Capture feature for auditing, it will make sure we have a copy of every event in blob storage in an efficient format. The folder structure is split by date, so we can have a process which runs regularly to clear out old data.

For our custom processing we'll use the partitioned consumer pattern:

- [processor/Program.cs](/src/eventhubs/processor/Program.cs) - looks complicated, but mostly it's just setting up the standard library, telling it which blob storage container to use for recording state; the _UpdateCheckpoint_ call is where we record how far we've processed

You can run single processor and it will read from all the partitions:

```
# print the connection string for the Event Hub:
az eventhubs namespace authorization-rule keys list -n RootManageSharedAccessKey --query primaryConnectionString -o tsv -g labs-eventhubs-consumers --namespace-name <eh-name>

# print the connection string for the Storage account:
az storage account show-connection-string --query connectionString -o tsv -g labs-eventhubs-consumers -n <eh-name>

# run the processor:
dotnet run --project ./src/eventhubs/processor -cs '<event-hub-connection-string>' -scs '<storage-account-connection-string>'
```

You'll see a log printed for every 100 events, listing the partitions the consumer is reading from.

> It may take a while before all 5 partitions are read, because the consumer won't read them all to start with.

When processing is finished, run the consumer again - do the same events get processed?

```
dotnet run --project ./src/eventhubs/processor -cs '<event-hub-connection-string>' -scs '<storage-account-connection-string>'
```

> Hopefully not :) 

The additional complexity in the code is all about recording what has already been processed. There's no guarantee an event won't be processed twice - a consumer could process a batch of events and then crash before it updates the checkpoint. In that case when the consumer restarts it would process those events again.

But we should be able to guarantee that every event will get processed and no events will get missed. This is called an _at least once_ processing guarantee.

## Lab

This library is all about processing at scale. Open two more terminal windows and run a processor in each - so you have three running in total. Then run the producer to create some more event batches. Do all the consumers do some processing? Explore the Storage Account to see how they share the work. 

Try stopping a processor midway through a batch. What happens to the rest of the events in that processor's partition? And what if you start a consumer with a different consumer group (using the `-g` parameter) - which events does it pick up?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y --no-wait -n labs-eventhubs-consumers
```