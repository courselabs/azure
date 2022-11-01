# Lab Solution

Start by running more subscribers for one subscription:

```
dotnet run --project src/servicebus/subscriber -topic broadcast -subscription desktop -cs '<subscriber-connection-string>'
```

The new subscriber doesn't start with a backlog of messages to process, because the other consumer has been working through the subscription.

New message batches are shared by the subscribers:

- the single `web` subscriber processes all the messages when they hit that subscription
- the two `desktop` subscribers share the incoming messages, just like with multiple subscribers on a queue

Add another publisher:

```
dotnet run --project src/servicebus/publisher -topic broadcast -cs '<publisher-connection-string>'
```

Both subscriptions get copies of all the messages from both publishers. The processing continues in the same way, with incoming messages shared between all the consumers for a subscription.
