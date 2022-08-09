# Azure Storage Accounts

New storage account, explore redundancy, security & performance

## Reference

- [Storage Account overview](https://docs.microsoft.com/en-gb/azure/storage/common/storage-account-overview)

- [Data redundancy in Azure](https://docs.microsoft.com/en-gb/azure/storage/common/storage-redundancy?toc=%2Fazure%2Fstorage%2Fblobs%2Ftoc.json)

- [`az storage` commands](https://docs.microsoft.com/en-us/cli/azure/storage?view=azure-cli-latest)

- [`az storage account` commands](https://docs.microsoft.com/en-us/cli/azure/storage/account?view=azure-cli-latest)


## Explore Storage Account options

In Portal, create new resource, search for storage account. Create :

- sa name needs to be globally unique
- select rg & region
- performance and redundancy

Local (within datacenter); zone-redundant (within region); geo-redundant (between regions); GZRS

- advanced: security & features
- data protection: soft deletes & versioning
- encryption: key management


## Create a storage account


az group create -n labs-storage  -l westeurope --tags courselabs=azure

az storage account --help

az storage account create --help


Create a zone-redundant storage account with standard performance (https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules#microsoftstorage)

```
# names have stricter rules than most Azure resources
az storage account create -g labs-storage  -l westeurope --sku Standard_ZRS -n labsstoragees
```

Open in the portal - one storage account can support multiple types of storage. Blob storage (Binary Large OBjects) is a simple file storage option - upload the file `document.txt` in this folder as a blob.

The Storage Account blade has an _Upload_ option in the main menu. Select that and you can browse to your local file and upload it.

> Blobs are stored in containers - you can have a multi-level hierarchy within each container.

## Upload and download blobs

You can manage storage from within the portal - click _Storage browser_ from the left nav and open _Blob containers_.

Navigate to the details of your uploaded blob. What is the URL of the file? Is is publicly accessible?

- open the `newcontainer` and you'll see `document.txt`. Click and you'll get an overview which includes the URL.

Use curl to download it:

curl -o download2.txt https://labsstoragees.blob.core.windows.net/newcontainer/document.txt

cat download2.txt

> XML error message...

New containers default to private access. Browse back to the container and select _Change access level_. Select Blob access and download again:

curl -o download2.txt https://labsstoragees.blob.core.windows.net/newcontainer/document.txt

cat download2.txt

Now you can read the file.

## Storage for VM disks

Create a VM with custom storage SKU:

az vm create -l westeurope -g labs-storage -n vm01 --image UbuntuLTS --size Standard_A1_v2 --storage-sku StandardSSD_ZRS


Check the storage accounts:

az storage account list -g labs-storage -o table

> No new account, only the one you originally created

[Managed disks](https://docs.microsoft.com/en-us/azure/virtual-machines/managed-disks-overview) aren't contained in a storage account. You can use unmanaged disks if you want to control the storage.

Create a premium storage account and a VM with the OS disk stored in that account - check the [VM types](https://docs.microsoft.com/en-us/azure/virtual-machines/sizes-general) to see which support premium storage.

```
az storage account create -g labs-storage  -l westeurope --sku Premium_LRS -n labsstoragediskes

az storage container create -n vm-disks --account-name labsstoragediskes

az vm create -l westeurope -g labs-storage -n vm04 --image UbuntuLTS --size Standard_D2as_v5  --use-unmanaged-disk --storage-container-name vm-disks --storage-account labsstoragediskes
```

Now browse to the new storage account - how is the disk stored?

> It is a VHD blob in the storage container. Doesn't show as a disk in the RG.

Open the managed disk from vm01 in the Portal - how does it compare to the VHD?

- Managed options - can change size & perf; export; view metrics

## Lab

secure the original SA so it can only  be accessed from your own IP address. confirm you can download the document.txt file; login to one of the VMs and confirm that it can't download the file.