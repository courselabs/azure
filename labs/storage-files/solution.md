There is no tier option in `storage share create`:

```
```

You need to use the alternative command - still creates the same sort of share - storage share-rm create:

```
az storage share-rm create --help
```

Try to create a Premium share in a standard SA and you'll get an error:

```
az storage share-rm create -n labs-premium --quota 100 --access-tier Premium --storage-account labsstoragefileses
```

> It's a weird error, but the issue is that a Premium share needs a Premium SA - you'll see that in the Portal if you try to create a new share.

New SA - needs to be Premium SKU & file storage kind:

```
az storage account create -g labs-storage-files  -l westeurope --sku Premium_LRS --kind FileStorage -n labsstoragefileslabes3

az storage share-rm create -n labs-premium --quota 100 --access-tier Premium --storage-account labsstoragefileslabes3
```

You use the share in the same way, but you get charged for the provisioned amount - i.e. you pay for 100GB SSD even if the share is empty. With standard you only pay for storage you use, the quota is a maximum.

This SA is specifically for File Storage - open in the Portal, there are no Blob or other options.

