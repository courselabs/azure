# Azure Cache for Redis

Redis is a popular open-source technology which is a combination of a message queue and a data store. It's very lean to run and has a straightforward programming interface. It's commonly used as a cache for non-critical data, or for async communication where reliable messaging is not needed. Azure Cache for Redis is a fully managed service which implements the Redis API and is a straight swap for a run-your-own Redis cluster.

In this lab we'll use Redis as both a data cache and a message queue and see what the managed Azure service provides

## Reference

- [Azure Redis overview](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview)

- [Redis developer docs](https://developer.redis.com)

- [Redis API commands](https://redis.io/commands/)

- [`az redis` commands](https://learn.microsoft.com/en-us/cli/azure/redis?view=azure-cli-latest)


## Create a Redis Cache

Create a new resource and search for _redis_ - there are lots of matches. 

Redis is an open-source project and different vendors package it to run on Azure. Select _Azure Cache for Redis_ (**not** _Azure Cache for Redis Enterprise & Flash_). Check the configuration options:

- your Redis instance will have a public DNS name will use the suffix `redis.cache.windows.net`
- the _cache type_ is the pricing tier and defines capacity & reliability
- Virtual Networks are available for higher tiers only
- you can choose an unsecured (non-TLS) connection, and the Redis version

We'll create a Redis instance in the CLI. Start with a Resource Group:

```
az group create -n labs-redis --tags courselabs=azure -l westeurope
```

Check the help text:

```
az redis create --help
```

You can use a JSON file for more advanced config options, but we don't need that level of control. 

📋 Create a basic SKU v6 Redis instance with size C0 and requiring TLS 1.2.

<details>
  <summary>Not sure how?</summary>

```
az redis create --sku Basic --vm-size c0 --minimum-tls-version 1.2 --redis-version 6 -g labs-redis -n <redis-name> 
```

</details><br/>

> It can take a while before the Redis instance is fully online

Open the Redis instance in the Portal - it will be there even if it hasn't finished creating yet. You'll see _Access keys_ with connection details, and lots of features not available with this SKU, including _Geo-replication_, _Cluster size_ and _Data persistence_.

Even without those features, a basic Redis cache can be a very powerful addition to your apps.

## Run the Pi app

We have a simple application to run which calculates Pi to a given number of decimal places:

- [pi/Program.cs](/src/pi/Program.cs) - the entrypoint for the application, has the ability to use Redis for a cache and for event publishing.

Try running the app (you'll need the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download)), calculating Pi to a large number of decimal places:

```
# no cache - takes a few seconds:
dotnet run --project ./src/pi -dp 1000
```

It will take a second or two, depending on how fast your CPU is. This is a compute-intensive operation, but the computed values never change - it will always give the same result for Pi with the same request.

> Repeat the command and you'll see the same result, but it's computed from scratch again so it will still take a few seconds.

This is a perfect scenario for a cache - where the data rarely (or never) changes, and the time to fetch it over the network is faster than calculating the result again.

We'll use Redis as our cache. Get the Redis access key (you can also see it in the Portal):

```
az redis list-keys -g labs-redis -n <redis-name>
```

That key gets used as the password for the Redis client. Run the app with the cache enabled **you'll need to set your own Redis DNS name and password**:

```
dotnet run --project src/pi -dp 1000 -usecache -cs '<redis-name>.redis.cache.windows.net:6380,password=<redis-key>,ssl=True,abortConnect=False' 
```

It still took a few seconds to get the response, because the cache was empty. After calculating it, this instance of the app stored the result in Redis, so it will be there for future instances to read.

> Redis is a shared cache - different instances of different services can use it to share data or publish events

## Check the data in Redis

In the Portal open the _Console_ from the  Redis blade.

This is the [Redis CLI](), embedded in the Portal and already connected to your Redis instance.

Check the data - the cache key is `pi-` followed by the number of decimal places:

```
GET pi-1000
```

You'll see the same output you saw when you ran the .NET app. 

> Run your app again and this time it will read the value from Redis and not need to recalculate

That should be faster, unless you have a very fast machine. If you do, try again with a larger number of decimal places (e.g. `-dp 100000`); that takes 10+ seconds to compute on my machine, but returns in a few seconds once it has been cached. 

The data in Redis is read-write from any process with access. Back in the console delete your cached data:

```
DEL pi-1000
```

The Redis commands are very curt. You'll see the response `1` if the value was deleted, or `0` if the key wasn't found. Run the app again and this time the cache is gone so it needs to re-calculate.

> The cache is not critical - the app works correctly without it, but responses take longer

This is a perfect use-case for Redis. In the basic tiers the Redis data isn't replicated or persisted, it's effectively in-memory in a single server. If it restarted the data would be lost, so that's no good for transactional data but it's fine for a cache.

## Subscribe for events

Redis also supports pub-sub messaging in the same instance that you use for data storage. It's not reliable messaging of the sort that you get with [Service Bus](), but it is fast and easy to use.

The Pi application can be set to publish events when it has computed a value. That's a good way to see if the cache is working, and it's also an event some other process might be interested in.

In the Redis console in the Portal you can subscribe to messages on the channel used by the Pi app:

```
SUBSCRIBE events.pi.computed
```

You'll see some messages printed confirming that the console is subscribed to the queue. Now it's listening for messages.

In your own terminal, calculate some Pi values:

📋 Run the Pi app a few more times for different decimal place values, using the `usecache` and `publishevents` flags and your Redis connection string.

<details>
  <summary>Not sure how?</summary>

These are new calculations so they will need to be computed each time:

```
dotnet run --project ./src/pi -dp 100 -usecache -publishevents -cs '<redis-name>.redis.cache.windows.net:6380,password=<redis-key>,ssl=True,abortConnect=False' 

dotnet run --project ./src/pi -dp 200 -usecache -publishevents -cs '<redis-name>.redis.cache.windows.net:6380,password=<redis-key>,ssl=True,abortConnect=False' 

dotnet run --project ./src/pi -dp 300 -usecache -publishevents -cs '<redis-name>.redis.cache.windows.net:6380,password=<redis-key>,ssl=True,abortConnect=False' 
```

</details><br/>

Check in the Azure Portal and you will see the events being received in the Redis console. Each event will print three lines which will look like this:

```
1) "message"
2) "events.pi.computed"
3) "Calculated Pi to: 200dp"
```

This is a demo app but you see this pattern in real applications. An intensive operation gets cached by storing it in Redis, and an event gets published to say the data is there in the cache. Consumers using that data subscribe to the event, so they know when to reload the data from the cache.
## Lab

Redis isn't a reliable message queue like Service Bus or Event Hubs. What happens if you have mutiple subscribers and then calculate Pi, do they all receive the event? What if there are no subscribers running when you publish events from the Pi app? 

Redis doesn't have an infinite amount of memory available, and it will start evicting old cached items if it gets full. Can you see in the Portal if that's happening? The CLI supports the Redis API for management commands too. Can you delete all the entries in the cache to reset it?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y --no-wait -n labs-redis 
```