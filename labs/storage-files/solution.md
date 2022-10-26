# Lab Solution

## Standard shares

You can change the capacity of a standard share up to 5TB - but that's the default if you don't set a different capacity at creation, so your share is already at the max.

With standard shares you get charged for how much data you are actually storing, and the capacity is just the limit you set. So if you have 1GB data stored in a share with 1TB capacity you will pay for 1GB.

## Premium shares

There is no tier option to select standard or premium in `storage share create`, you need to use an alternative command - which still creates the same sort of share:

```
az storage share-rm create --help
```

Try to create a Premium share in a standard SA and you'll get an error:

```
az storage share-rm create -n labs-premium --quota 100 --access-tier Premium --storage-account <sa-name>
```

> You'll see a weird unhelpful error about HTTP headers, but the issue is that a Premium share needs a Premium SA - you'll see that in the Portal if you try to create a new share

So create a new SA - needs to be Premium SKU & flagged for file storage:

```
az storage account create -g labs-storage-files  -l westeurope --sku Premium_LRS --kind FileStorage -n <premium-sa-name>

az storage share-rm create -n labs-premium --quota 100 --access-tier Premium --storage-account <premium-sa-name>
```

You use the share in the same way **but you get charged for the provisioned amount**. If you have 1GB of data in a premium share with 100GB capacity, you pay for 100GB. 

This SA is specifically for File Storage - open in the Portal and you'll see there are no Blob or other options.