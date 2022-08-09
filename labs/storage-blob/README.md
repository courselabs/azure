# Blob Storage

## Reference

- [Blob storage access tiers](https://docs.microsoft.com/en-us/azure/storage/blobs/access-tiers-overview)

## Managing Blob Storage with the CLI

az group create -n labs-storage-blob  -l westeurope --tags courselabs=azure

az storage account create -g labs-storage-blob  -l westeurope --sku Standard_LRS -n labsstorageblobes

az storage container create -n labs  -g labs-storage-blob --account-name labsstorageblobes

az storage blob upload-batch -d labs -s ./labs --dryrun -o table --account-name labsstorageblobes

- restrict it just to the markdown docs

az storage blob upload-batch -d labs -s ./labs --dryrun -o table --pattern '*.md' --account-name labsstorageblobes

az storage blob upload-batch -d labs -s ./labs --pattern '*.md' --account-name labsstorageblobes


> Output shows generated eTag and Last Modified dates - for HTTP caching


Check the exercises files are there:

az storage blob directory list -c labs -d 'storage-blob' -o table --account-name labsstorageblobes


> Preview; az cli has modular extensions; also deprecated :)

az storage blob show --container-name labs --name 'storage-blob/README.md' -o table --account-name labsstorageblobes

- case sensitive

az storage blob show --container-name labs --name 'storage-blob/readme.md' -o table --account-name labsstorageblobes

> BlobNotFound

## Shared Access Tokens

Try to download the README doc:

# fixed pattern for blob URL: https://<account-name>.blob.core.windows.net/<container>/<blob-name>

curl -o download.md https://labsstorageblobes.blob.core.windows.net/labs/storage-blob/README.md

cat download.md

> Does not exist - container not enabled for public blobs

Open the README.md blob in the Portal; click the ellipsis and _Generate SAS_. Create a SAS key for read-only access to the blob, which is valid for 1 hour.

Copy the blob SAS URL, it will look like this:

_https://labsstorageblobes.blob.core.windows.net/labs/storage/README.md?sp=r&st=2022-08-09T13:35:54Z&se=2022-08-09T14:35:54Z&spr=https&sv=2021-06-08&sr=b&sig=3HH6hghHJSwxkmXIglyUdgZC9C1jCQk%2BaZ%2BhIVdUpQE%3D`

Use curl to download the file using the SAS URL:

curl -o download2.md 'https://labsstorageblobes.blob.core.windows.net/labs/storage/README.md?sp=r&st=2022-08-09T13:35:54Z&se=2022-08-09T14:35:54Z&spr=https&sv=2021-06-08&sr=b&sig=3HH6hghHJSwxkmXIglyUdgZC9C1jCQk%2BaZ%2BhIVdUpQE%3D'

cat download2.md

Can safely share - after expiry date, no access. But simple SAS cannot be revoked.

Can manage SAS better with a [Shared Access Policy]():

Create a read-only shared access policy:

az storage container policy create --help 

az storage container policy create -n labs-reader --container-name labs --permissions r --account-name labsstorageblobes

Now create a SAS token for the blob, backed by the access policy:

```
az storage blob generate-sas --help

# powershell
$expiry=$(Get-Date -Date (Get-Date).AddHours(1) -UFormat +%Y-%m-%dT%H:%MZ)

# zsh (use -d on Bash)
expiry=$(date -u -v+1H '+%Y-%m-%dT%H:%MZ')

az storage blob generate-sas -n 'storage-blob/README.md' --container-name labs --policy-name labs-reader --full-uri --expiry $expiry --account-name labsstorageblobes 
```

Verify you can download the file:

```
curl -o download3.md "https://labsstorageblobes.blob.core.windows.net/labs/storage-blob/README.md?se=2022-08-09T16%3A14Z&sv=2021-04-10&si=labs-reader&sr=b&sig=DT9nobaYNI4zMGV1YVQ2GvZlbZQDpA2C/5OOgm4ErlU%3D"

cat download3.md
```

> Correct content

Now remove the policy:


az storage container policy delete -n labs-reader --container-name labs --account-name labsstorageblobes

And try the download again:

```
curl -o download4.md "https://labsstorageblobes.blob.core.windows.net/labs/storage-blob/README.md?se=2022-08-09T16%3A14Z&sv=2021-04-10&si=labs-reader&sr=b&sig=DT9nobaYNI4zMGV1YVQ2GvZlbZQDpA2C/5OOgm4ErlU%3D"

cat download4.md
```

> Not authenticated - SAS not valid without policy; 

can also edit policy permissions and generate SAS at different levels

## Access Tiers

Hot - immediate access, good perf, highest cost; cool - immediate, lower perf & cost; archive - cheapest but offline, needs to be restored to hot or cool before available.

az storage blob set-tier --container-name labs --name 'storage-blob/README.md' --tier Cool --account-name labsstorageblobes


> Check blob in the Portal - now Cool, can still download (Portal you have Azure auth, not a public download)

NOw archive the blob:

az storage blob set-tier --container-name labs --name 'storage-blob/README.md' --tier Archive --account-name labsstorageblobes

- Check again in the Portal - now you can't download because the blob is archived


## Lab

Hierarchy of SA to container to blob. Set all the blobs in the labs container to the cool access tier.

