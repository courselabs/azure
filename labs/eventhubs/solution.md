# Lab Solution

There are two key parts to this:

- consumers can read events in a specific partition, rather than reading from any partition
- consumers can read event within a partition from an offset, rather than starting from the beginning

You can run at scale by having multiple consumers, each reading from a different partition. They'll be working through the events in parallel.

As the consumer processes each event it can store the offset. This is unlike Service Bus messages, where the consumer flags back to the Service Bus that the message has been processed. Instead the consumer keeps its own record of how far in the stream it has got.

The offset is calculated by Event Hubs for each partition, and is included in the event metadata the consumer receives. When the consumer starts, it's configured to use one partition and looks up it's own record of the last offset it processed. It connects to the Event Hub asking to start receciving events in the partition from the given offset, so it starts where it left off.