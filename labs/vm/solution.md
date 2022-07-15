# Lab Solution

Print the details of the VM:

```
az vm show -g labs-vm -n vm01
```

In there you'll see a section for `osDisk` which includes the ID and name. That's nested inside the `storageProfile` object. You can filter the output to store the disk name in a variable:

```
# PowerShell:
$diskName=$(az vm show -g labs-vm -n vm01 --query "storageProfile.osDisk.name" -o tsv)

# sh:
diskName=$(az vm show -g labs-vm -n vm01 --query "storageProfile.osDisk.name" -o tsv)
```

Now you can use the [az disk]() commands to show all the details of the disk:

```
az disk show --help

az disk show -g labs-vm -n $diskName
```

You'll see the IOPS in the field `diskIopsReadWrite` - different disk types have different performance levels.

Now delete the VM:

```
az vm delete -g labs-vm -n vm01 --yes
```

Check the Resource Group in the Portal - you'll see the disk and all the network resources are retained after the VM is deleted.

> Back to the [exercises](README.md).