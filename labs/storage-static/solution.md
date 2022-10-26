# Lab Solution

In the Portal you can open the CDN endpoint from the Storage Account. The top menu lets you refresh content with _Purge_ - you can do that for all content, or a specific path.

Or in the CLI:

```
az cdn endpoint purge --content-paths '/index.html' -g labs-storage-static --profile-name labs-storage-static -n <cdn-domain>
```

> This may take a while - usually you would use the --no-wait flag

Check the CDN domain after this. If your content hadn't refreshed before the purge, it should be updated now.
