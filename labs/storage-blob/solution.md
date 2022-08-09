
Try using a wildcard for all blobs:

```
# this will fail:
az storage blob set-tier --container-name labs --name '*' --tier Cool --account-name labsstorageblobes
```

Try setting it at the account level:

```
az storage account update --access-tier Cool --name labsstorageblobes -g labs-storage-blob
```

This does update all the blobs because they were uploaded without specifying the access tier - you'll seel _Cool (inferred)_ in the Portal, because the tier inherits the default setting in the storage account.

If you upload blobs and set the access tier explicitly, changing the account tier won't affect them - you'll need to iterate over them and set the tiers in a loop.