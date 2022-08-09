# Static Websites with Azure Storage Blobs


## Reference

- [Microsoft Learn: Deploy your Babylon.js project to the public web](https://docs.microsoft.com/en-us/learn/modules/create-voice-activated-webxr-app-with-babylonjs/9-exercise-deploy-babylonjs-project-to-public-web?pivots=vr) - part of a larger module, this exercise covers static web hosting


## Create a static website Storage Account

Normal storage account:

```
az group create -n labs-storage-static  -l westeurope --tags courselabs=azure

az storage account create -g labs-storage-static  -l westeurope --sku Standard_LRS -n labsstoragestatices
```


Extra config for the blob service:

```
az storage blob service-properties update  --static-website --404-document 404.html --index-document index.html --account-name labsstoragestatices
```

Open in the Portal - browse to _Static website_:

- the base URL is different from the standard blob URL
- there is a new container called `$web` for the content

Browse to the URL, what do you see?

> e.g. https://labsstoragestatices.z6.web.core.windows.net/ - standard 404, _Requested content does not exist_


Upload the contents of the `labs/storage-static/web` directory to the container and browse again:

- portal or CLI

```
az storage blob upload-batch -d '$web' -s labs/storage-static/web --account-name labsstoragestatices
```

Refresh the browser - you'll see the home page; browse to any other path (e.g. /missing) and you'll get the customized 404

The blobs themselves are not publicly available - find the blob URL of the index.html file in the portal and try to download it:

```
curl -o download.html 'https://labsstoragestatices.blob.core.windows.net/$web/index.html'

cat download.html
```

> Resource does not exist message

Try with the public URL:

```
curl -o download2.html https://labsstoragestatices.z6.web.core.windows.net/index.html

cat download2.html
```


## Replication to a second region

Change the SA to use read-only globally redundant storage:

```
az storage account update -g labs-storage-static --sku Standard_RAGRS -n labsstoragestatices
```

In the output you'll see the secondary location and a list of secondary endpoints. You can access the site from the secondary too, and the storage cost is lower with RA than full GRS:

```
curl -v https://labsstoragestatices-secondary.z6.web.core.windows.net/
```

> You'll probably get an error - it takes a while for data to be synchronized to the secondary region

Open the Portal and the _Geo-replication_ tab, and you can see the status of the replication.


## Global replication with CDN

Azure CDN is a separate service which you can use as a front-end to static websites.

In the Portal open _Azure CDN_ for the storage account - you can create a new CDN endpoint from here, or with the CLI:

```
az cdn profile create --sku Standard_Microsoft -g labs-storage-static -n labs-storage-static

az cdn endpoint create  -g labs-storage-static --profile-name labs-storage-static --origin <primary-endpoint-domain> --origin-host-header <primary-endpoint-domain> -n <unique-domain>

# e.g.
az cdn endpoint create  -g labs-storage-static --profile-name labs-storage-static --origin labsstoragestatices.z6.web.core.windows.net --origin-host-header labsstoragestatices.z6.web.core.windows.net  -n static-blob-es
```

Check the status in the Portal. Browse to https://<unique-domain>.azureedge.net

Update - can take a while to replicate:

az storage blob upload-batch -d '$web' -s labs/storage-static/web2 --overwrite  --account-name labsstoragestatices


Check at the primary domain - new content shows; check on CDN - still old

Force reload with purge:

az cdn endpoint purge --content-paths '/index.html' -g labs-storage-static --profile-name labs-storage-static -n static-blob-es 

> May take a while - usually use --no-wait flag; check site again on completion