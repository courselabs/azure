# Static Websites with Azure Storage Blobs

One other use for Azure Storage is for static web content. You can upload HTML files and static assets as blobs, and configure the blob container for public web access. You get a fast, scalable website with no web server to manage.

In this lab we'll see how to host a website on blob storage, and scale it with Microsoft's Content Delivery Network (CDN).

## Reference

- [Static websites in Storage Accounts](https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-static-website)

- [What is a CDN?](https://learn.microsoft.com/en-us/azure/cdn/cdn-overview)

- [Microsoft CDN point-of-presence locations](https://learn.microsoft.com/en-us/azure/cdn/cdn-pop-locations?toc=%2Fazure%2Ffrontdoor%2FTOC.json)

- [Microsoft Learn: Deploy your Babylon.js project to the public web](https://docs.microsoft.com/en-us/learn/modules/create-voice-activated-webxr-app-with-babylonjs/9-exercise-deploy-babylonjs-project-to-public-web?pivots=vr) - part of a larger module, this exercise covers static web hosting


## Deploy a Static Website

Start by creating a Resource Group and a normal storage account:

```
az group create -n labs-storage-static --tags courselabs=azure -l westeurope

az storage account create -g labs-storage-static --sku Standard_LRS -n <sa-name>
```

📋 Use a `blob service-properties update` command to configure the storage account for a static website.

<details>
  <summary>Not sure how?</summary>

You need to use the `static-website` flag and pass the file names for the main (index) page, and the 404 page:

```
az storage blob service-properties update  --static-website --404-document 404.html --index-document index.html --account-name <sa-name>
```

</details><br/>

> You don't need any content uploaded before you enable the static website settings

Open the Storage Account in the Portal - browse to the _Static website_ blade. You'll see:

- the base URL is different from the standard blob URL
- there is a new container called `$web`, which is where the web content will need to be uploaded

Browse to the URL, what do you see?

> The site responds, but with a standard 404 not found error, _Requested content does not exist_

📋 Upload the contents of the `labs/storage-static/web` directory to the container and browse again

<details>
  <summary>Not sure how?</summary>

You can upload multiple files in the Portal, or use the batch upload in the CLI:

```
az storage blob upload-batch -d '$web' -s labs/storage-static/web --account-name <sa-name>
```

</details><br/>

Refresh the browser - you'll see the home page; browse to any other path (e.g. /missing) and you'll get the customized 404

The blobs themselves are not publicly available - find the blob URL of the index.html file in the portal and try to download it:

```
curl -o download.html 'https://<sa-name>.blob.core.windows.net/$web/index.html'

cat download.html
```

> You'l get an XML error file. The pages need to be accessed via the static website domain

Try with the public URL instead:

```
curl -o download2.html https://<static-web-domain>/index.html

cat download2.html
```

## Replication to a second region

Static websites are a good scenario for using a higher level of redundancy, to make sure your data is safe and your site is always available.

Change the Storage Account to use _read-only globally redundant storage_ (RA-GRS), which means the content is replicated to a second region which can be used for reads:

```
az storage account update -g labs-storage-static --sku Standard_RAGRS -n <sa-name>
```

In the output you'll see the secondary location and a list of secondary endpoints, including one for the static website. You can access the site from the secondary too, and the storage cost is lower with RA than full GRS:

```
curl -v https://<secondary-web-endpoint>/
```

> You'll probably get an error - it takes a while for data to be synchronized to the secondary region

Open the Storage Account in the Portal and check the _Redundancy_ tab, and you can see the status of the replication.

It can take a long time for the sync to complete in geo-replication. You can check the status by querying the last sync time:

```
az storage account show -g labs-storage-static --expand geoReplicationStats --query geoReplicationStats.lastSyncTime -o tsv -n <sa-name>
```

> You'll see the message _Last sync time is unavailable_ if the account is still syncing

You could set up your DNS provider with both endpoints, so if one was unavailable then the website would get served from the other region. An alternative is to use CDN.

## Global replication with CDN

Azure CDN is a separate service which you can use as a front-end to static websites. It's a global network where content is copied to multiple edge locations, so when users browse they get a response from a local area.

In the Portal open _Azure CDN_ for the storage account - you can create a new CDN endpoint from here, or with the CLI:

```
az cdn profile create --sku Standard_Microsoft -g labs-storage-static -n labs-storage-static

az cdn endpoint create  -g labs-storage-static --profile-name labs-storage-static --origin <static-website-domain> --origin-host-header <static-website-domain> -n <cdn-domain>
```

Check the status in the Portal. Browse to `https://<cdn-domain>.azureedge.net`. You may see an error page saying it takes a while for the CDN to be ready - the data is being replicated across the network at this point.

Keep refreshing. When you can see your site, that means CDN is populated and the data is being served from somewhere close to you.

📋 Change the content of the site by uploadling `labs/storage-static/web` directory to the static website container 

<details>
  <summary>Not sure how?</summary>

```
az storage blob upload-batch -d '$web' -s labs/storage-static/web2 --overwrite  --account-name <sa-name>
```

</details><br/>

Check your various URLs:

- you should see the content straight away at the original static website URL
- the secondary endpoint could be updated almost as quickly - **assuming the original sync has completed**
- the CDN can take the longest to refresh, so it might show the old content for a few minutes

## Lab

The purpose of the CDN is to cache content which is has a heavy read profile, but is not updated so regularly. The caching is quite aggressive, and sometimes you might need to force a page to be refreshed in the CDN. How would you do that for the `index.html` page?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y -n labs-storage-static --no-wait
```