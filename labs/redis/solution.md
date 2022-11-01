# Lab Solution

## Pub-sub messaging

If you start with one CLi sunscriber, you can exit that so there is nothing listening on the queue. Then run your Pi app a few times with new `-dp` values, which will store the data in Redis and publish events.

The app works in the same way, it doesn't fail because there are no subscribers.

Then subscribe to the events again in the Redis CLI:

```
SUBSCRIBE events.pi.computed
```

> There are none. Redis does not store messages if there are no subscribers. 

Unlike Service Bus, if there's nothing listening on a queue when messages are published, they get lost forever.

Try opening a new browser window with the Redis CLI and subscribe again:

```
SUBSCRIBE events.pi.computed
```

Run your Pi app some more times with new `-dp` values and check back to the consoles. You'll find each subscriber gets a copy of the message.

> This is also different to Service Bus - where if there are multiple subscribers to a queue then the messages are shared between them

Redis only provides pub-sub messaging, where all subscribers get a copy of the message. If you want to run multiple instance of a subscriber to support scale, you need to manage the workload in your own code so the processing isn't duplicated.

## Clearing the cache

On the homepage for the Redis instance in Azure you can see the Redis memory usage. It will be a tiny amount for our cached Pi results, but if you use Redis extensively then you'll want to keep track of this during some performance testing.

If you ever need to completely clear out the cache, you can do it in the CLI with the flush command:

```
FLUSHDB
```

That removes everything (try some `GET` commands to verify it). The cache is still operational though, so you can run your Pi app again and it will insert a new item.

