
Run any number of publishers:


```
cd src/queues/queue-publisher

dotnet run -cs "$(az servicebus namespace authorization-rule keys list -n RootManageSharedAccessKey -g labs-servicebus  --query primaryConnectionString -o tsv --namespace-name labsservicebuses)"
```


Run a subscriber which does not ack:

```
cd src/queues/queue-subscriber

dotnet run -ack False -cs "$(az servicebus namespace authorization-rule keys list -n RootManageSharedAccessKey -g labs-servicebus  --query primaryConnectionString -o tsv --namespace-name labsservicebuses)"
```

> Messages are returned to the queue and other subscribers (or the same one) get them again. TODO - PeekLock duration 5minutes