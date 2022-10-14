# Event Hubs

Competing consumer pattern vs partition consumer - locks & high water marks. Need to store checkpoint outside app process, client uses blob storage.


## Reference

- [Balance Event Hubs partition load](https://learn.microsoft.com/en-us/azure/event-hubs/event-processor-balance-partition-load)

- [Competing consumer pattern](https://learn.microsoft.com/en-us/previous-versions/msp-n-p/dn568101(v=pandp.10))

- [`az eventhubs eventhub consumer-group` commands](https://learn.microsoft.com/en-us/cli/azure/eventhubs/eventhub/consumer-group?view=azure-cli-latest)

## Create an Event Hub Namespace & Storage Acccount

Create an RG and an Event Hub namespace:

```
az group create -n labs-eventhubs-consumers -l westeurope --tags courselabs=azure

# create a namespace - standard SKU for multiple consumer groups:
az eventhubs namespace create --min-tls 1.2 --capacity 2 --location westeurope --sku Standard -g labs-eventhubs-consumers -n labseventhubsconsumerses # <unique-dns-name>

# create storage account for consumer state:
az storage account create  -l westeurope --sku Standard_ZRS -g labs-eventhubs-consumers -n labseventhubsconsumerses # <unique-dns-name>

# and two blob containers in the storage account:
az storage container create -n checkpoints  -g labs-eventhubs-consumers --account-name labseventhubsconsumerses

az storage container create -n devicelogs  -g labs-eventhubs-consumers --account-name labseventhubsconsumerses
```

> There's no direct link between the Event Hub and the storage account - they are only brought together in the consumer code.

Open the Event Hub namespace in the Portal - there's one extra option in the left menu: _Networking_. How might you use private networking with Event Hubs?

## Create an Event Hub & Consumer Groups

We'll create a new Event Hub with more partitions, to enable consumers at scale:

```
# standard SKU allows for longer message retention:
az eventhubs eventhub create --name devicelogs --partition-count 5 --message-retention 2 -g labs-eventhubs-consumers --namespace-name labseventhubsconsumerses # <unique-dns-name>
```

We can create multiple consumer groups in this tier - you would use different groups for different features. They can run at different levels of scale (e.g. business processing with multiple consumers and audit log with a single consumer):

```
# check the options - this is at the Event Hub level, not the namespace:
az eventhubs eventhub consumer-group create --help

az eventhubs eventhub consumer-group create -n processing --eventhub-name devicelogs -g labs-eventhubs-consumers --namespace-name labseventhubsconsumerses # <unique-dns-name>

az eventhubs eventhub consumer-group create -n auditing --eventhub-name devicelogs -g labs-eventhubs-consumers --namespace-name labseventhubsconsumerses # <unique-dns-name>
```

> Check in the Portal - the two consumer groups are listed, but there is nothing you can do with them.

Consumer groups are a mechanism for isolating data reads, but it's the consumer's responsibility to manage the reads.

## Publish Events and Capture

Publish a chunk of data to the Event Hub

- same processor, no difference in publishing semantics

```
# get the connection string:
az eventhubs namespace authorization-rule keys list -n RootManageSharedAccessKey --query primaryConnectionString -o tsv -g labs-eventhubs-consumers --namespace-name labseventhubsconsumerses # <unique-dns-name>

# this will send 100 batches of 50 messages:
dotnet run --project ./src/eventhubs/producer -ProducerCount 100 -BatchSize 50 -cs '<connection-string>'
```

Open the Event Hub in the Portal and set up Capture:

- choose the Avro output format
- slide the time window and size window down to the minimum
- tick _Do not emit empty files when no events occur during the Capture time window_
- choose your `devicelogs` blob storage container

Save the changes and the open the Storage Account. In the container you should see your events stored (they may take a couple of minutes to come through). What does the folder structure signify? Open one of the Avro files, what does it contain?

## Run Processing Consumers

- src/eventhubs/processor/Program.cs

A single processor will read from all the partitions:

```
# print the connection string for the Storage account:
az storage account show-connection-string --query connectionString -o tsv -g labs-eventhubs-consumers -n labseventhubsconsumerses # <unique-dns-name>

# run the processor:
dotnet run --project ./src/eventhubs/processor -cs '<event-hub-connection-string>' -scs '<storage-account-connection-string>
```

You'll see a log printed for every 100 messages, listing the partitions the consumer is reading from.

> It may take a while before all 5 partitions are read, because the consumer won't read them all to start with.

When processing is finished, run the consumer again - do the same events get processed?

```
dotnet run --project ./src/eventhubs/processor -cs '<event-hub-connection-string>' -scs '<storage-account-connection-string>
```

## Lab

This library is all about processing at scale. Stop your processor, pen two more terminal windows and run a consumer in each - so you have three running in total. Then run the producer to create some more message batches. Do all the consumers do some processing? Explore the Storage Account to see how they share the work. 

Try stopping a processor midway through a batch. What happens to the rest of the message in that processor's partition? And what if you start a consumer with a different consumer group (-g parameter) - which messages does it pick up?