# Lab Solution

You can download blobs in the hot and cool tiers.

Change to the archive tier using the CLI:

```
az storage blob set-tier --container-name labs --name 'storage-blob/README.md' --tier Archive --account-name <sa-name>
```

Check that blob in the Portal - now you can't download it because archived blobs are meant for long-term backups which don't need immediate access.

You need to _rehydrate_ the blob to the cool or hot tier before it can be downloaded.

> The Portal shows you whether a blob can be downloaded - and it tells you that rehydrating can take hours :)