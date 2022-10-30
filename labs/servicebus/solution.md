# Lab Solution

You can run any number of publishers:

```
dotnet run --project src/servicebus/publisher -cs '<connection-string>'
```

Messages are still shared between all the subscribers. You'll see each subscriber processing messages from each publisher.

If you want to check reliability, the easiest thing is to quit all your existing subscribers, but leave the publisher(s) running.

Then run a single subscriber which does not ack:

```
dotnet run --project src/servicebus/subscriber -ack False -cs '<connection-string>' 
```

You'll see logs that messages are processed, but the acknowledgement logs are gone.

Stop the publisher(s) and wait for the subscriber to finish processing the batches. Now you should have a fixed set of unacknowledged messages still in the queue.

Stop the subscriber and replace it with a new instance:

```
dotnet run --project src/servicebus/subscriber -ack False -cs '<connection-string>' 
```

You should see the same batches of messages being processed. It may take a few minutes to see them all because Service Bus uses a timeout mechanism to make sure messages should really be delivered again,

This instance also fails to ACK the messages, so there still not flagged as processed in the Service Bus. Replace the subscriber with one which does ACK the messages:

```
dotnet run --project src/servicebus/subscriber -cs '<connection-string>' 
```

Now the messages get ACKd and they won't be sent to any other subscribers.