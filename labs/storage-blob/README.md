# Blob Storage

You can use blob storage as your own personal Dropbox alternative, but it's also a powerful storage backend for applications. If you have a scenario where users can upload files, it's better to store them in blob storage than in a relational database. You can even store JSON files in blobs as a cheap way to manage reference data in your app.

In this lab we'll cover some of the more advanced features of blobs, like access tokens and storage tiers.

## Reference

- [SAS tokens](https://learn.microsoft.com/en-us/azure/cognitive-services/translator/document-translation/create-sas-tokens?tabs=Containers)

- [Blob storage access tiers](https://docs.microsoft.com/en-us/azure/storage/blobs/access-tiers-overview)

- [`az storage container` commands](https://learn.microsoft.com/en-us/cli/azure/storage/container?view=azure-cli-latest)

- [`az storage blob` commands](https://learn.microsoft.com/en-us/cli/azure/storage/blob?view=azure-cli-latest)

## Managing Blob Storage with the CLI

We explored blob storage using the Portal in [Storage Account lab](/labs/storage/README.md). Now we'll see the CLI tooling for blobs.

📋 Create a Resource Group called `labs-storage-blob` and a Storage Account with standard locally redundant storage.

<details>
  <summary>Not sure how?</summary>

Remember the SA name can only be lowercase letters and numbers:

```
az group create -n labs-storage-blob --tags courselabs=azure -l westeurope 

az storage account create -g labs-storage-blob  -l westeurope --sku Standard_LRS -n <sa-name>
```

</details><br/>

Create a blob container where we can upload some files:

```
az storage container create -n labs  -g labs-storage-blob --account-name <sa-name>
```

The CLI has the `upload-batch` command which you can use to upload files to blob storage in bulk. It has a useful `dryrun` flag which tells you what it would do without actually doing it:

```
# see what would happen if you uploaded the whole labs folder:
az storage blob upload-batch -d labs -s ./labs --dryrun -o table --account-name <sa-name>
```

> The output tells you the target URL and file type for each blob it would upload.

Does the batch upload preserve the file paths?

📋 Use the same command with an additional parameter so only the markdown files (`*.md`) get uploaded. Use a dry-run first and then do the actual upload.

<details>
  <summary>Not sure how?</summary>

The `pattern` parameter lets you filter the files to upload:

```
az storage blob upload-batch -d labs -s ./labs --dryrun -o table --pattern '*.md' --account-name <sa-name>
```

And without the `dry-run` flag to upload:

```
az storage blob upload-batch -d labs -s ./labs --pattern '*.md' --account-name <sa-name>
```

</details><br/>

> Output shows each blob URL, generated eTag and Last Modified dates - those are used for HTTP caching

Check the exercises files are there with a directory list - this will print out the files in the `storage-blob` directory in the `labs` container:

```
az storage blob directory list -c labs -d 'storage-blob' -o table --account-name <sa-name>
```

> This is a deprecated command - the CLI evolves so you need to check if any commands get removed when you upgrade to a new version

📋 Use the `storage blob show` command to print information about the readme file in the `storage-labs` folder.

<details>
  <summary>Not sure how?</summary>

Blob file names are case-sensitive:

```
az storage blob show --container-name labs --name 'storage-blob/README.md' -o table --account-name <sa-name>
```

If you try `storage-blob/readme.md` instead then you'll get an `ErrorCode:BlobNotFound` response.

</details><br/>

The output only shows the file metadata, not the content.

## Shared Access Tokens & Policies

All blobs have a public URL which you can use if you do want to download the content. The URL is a standard pattern: `https://<sa-name>.blob.core.windows.net/<container>/<blob-name>`.

Try to download the README doc:

```
curl -o download.md https://<sa-name>.blob.core.windows.net/labs/storage-blob/README.md

cat download.md
```

> The output is an XML error string - the container is not enabled for public blobs

You can give someone access to the blob without making it public by creating a _Shared Access Signature_ (SAS token), which authorizes read access to the blob.

Open the `storage-blob/README.md` blob in the Portal; click the ellipsis and select _Generate SAS_. Create a SAS key for read-only access to the blob, which is valid for 1 hour.

Copy the blob SAS URL you see in the Portal, it will look like this:

_https://labsstorageblobes.blob.core.windows.net/labs/storage-blob/README.md?sp=r&st=2022-10-26T20:17:20Z&se=2022-10-26T21:17:20Z&spr=https&sv=2021-06-08&sr=b&sig=3b1TVwRMsgNHC%2BKE0tkR1VcqD0897%2BfbBJKfppfJ3B8%3D_

Use curl to download the file using the SAS URL:

```
curl -o download2.md '<blob-url-with-sas-token>'

cat download2.md
```

You'll see the contents this time. 

You can safely share that SAS token - after the expiry date, it's of no use and the blob won't be accessible. But a simple SAS token cannot be revoked, it can be used until it expires.

If you want better control for sharing blobs, you can manage SAS tokens with a [stored access policy](https://learn.microsoft.com/en-us/rest/api/storageservices/define-stored-access-policy):

```
# create a read-only policy:

az storage container policy create -n labs-reader --container-name labs --permissions r --account-name <sa-name>
```

Now create a SAS token for the blob, backed by the access policy. Expiry date needs to be in the format _YYYY-MM-DDTHH:MMZ_ e.g. _2022-10-30T01:00Z_

```
az storage blob generate-sas --help

# you'll get an error if your date the format is not valid, but not if the date is in the past:

az storage blob generate-sas -n 'storage-blob/README.md' --container-name labs --policy-name labs-reader --full-uri --expiry '2022-10-30T01:00Z' --account-name <sa-name> 
```

Verify you can download the file using the new SAS token:

```
curl -o download3.md "<blob-uri-with-sas-token>"

cat download3.md
```

You'll see the correct content - the token is within the expiry date, and the policy allows read access.

Now remove the policy:

```
az storage container policy delete -n labs-reader --container-name labs --account-name <sa-name>
```

And try the download again **using the same URI and token as before**:

```
curl -o download4.md "<blob-uri-with-sas-token>"

cat download4.md
```

> Now you'll see an XML authentication failure message - the SAS token is not valid without the policy

Stored access policies let you revoke access, so even if the SAS token is known, it can't be used.

## Lab

Blob storage doesn't usually need high performance, and Azure has _access tiers_ to let you get the best mix of performance and storage cost. _Hot_ access is faster but expensive; _cool_ is cheaper but slower and _archive_ is cheapest.

Change the readme for this lab to the archive tier and try to download it. What do you need to do to gain access to archived blobs? 

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y -n labs-storage-blob --no-wait
```