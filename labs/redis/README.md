# Azure Cache for Redis

Redis is a popular open-source technology which is a combination of a messaging service and a data store. It's very lean to run and has a straightforward programming interface. It's very commonly used as a cache for non-critical data, or for async communication where reliable messaging is not needed. Azure Cache for Redis is a fully managed service which implemetns the Redis API and is a straight swap for a run-your-own Redis cluster.

## Reference

- [Azure Redis overview](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-overview)

- [Redis developer docs](https://developer.redis.com)

- [Redis CLI commands](https://redis.io/docs/manual/cli/)

- [`az redis` commands](https://learn.microsoft.com/en-us/cli/azure/redis?view=azure-cli-latest)


## Create a Redis Cache

Create a new resource and search for _redis_ - there are lots of matches. Redis is an open-source project and different vendors package it to run on Azure. Select _Azure Cache for Redis_ (**not** _Azure Cache for Redis Enterprise & Flash_). Check the configuration options:

- Public DNS name will use the suffix `redis.cache.windows.net`
- Cache type is the pricing tier and defines capacity & reliability
- Virtual networks are available for higher tiers only
- You can choose an unsecured (non-TLS) connection, and the Redis version

We'll create a Redis instance in the CLI:

```
az group create -n labs-redis -l westeurope --tags courselabs=azure

# note that you can use a JSON file for more advanced config options:
az redis create --help

# create a minimal instance with Redis v6:
az redis create --sku Basic --vm-size c0 --minimum-tls-version 1.2 --redis-version 6 -g labs-redis -n labsredises # <unique-dns-name>
```

## Run the Pi app

```
# no cache - takes a few seconds:
dotnet run --project ./src/pi -dp 1000
```

Get the Redis connection string - from the Portal, or build it up from the CLI:

```
az redis list-keys -g labs-redis -n labsredises # <unique-dns-name>

dotnet run --project ./src/pi -dp 1000 -cs 'labsredises.redis.cache.windows.net:6380,password=I5BB5bfjXj4ymvfwL3viY82G1cEHO2XCTAzCaPkvPF8=,ssl=True,abortConnect=False' 
```

## Check the data in Redis

In the Portal open the _Console_ from the  Redis blade.

This is the Redis CLI, already connected to your Redis instance.

Check the data:

```
```