# Virtual Machines - Windows

VMs are useful as workstation machines as well as servers. It's useful to have a dev machine you can access from anywhere - you can set a powerful VM with all the dev tools you need, and only pay when you're actually using it.

In this lab we'll see how to create a Windows VM and manually configure with standard dev tools, by connecting to the VM and running a setup script.

## Reference

- [Azure Virtual Machine docs](https://docs.microsoft.com/en-gb/azure/virtual-machines/)

- [`az vm` commands](https://docs.microsoft.com/en-us/cli/azure/vm?view=azure-cli-latest)

- [`az vm image` commands](https://docs.microsoft.com/en-us/cli/azure/vm/image?view=azure-cli-latest)


## Explore Windows VMs in the Portal

Open the Portal and search to create a new Virtual Machine resource. Change the image to use a Windows OS; what changes?

- authentication model switches username + password
- the default incoming ports will allow 3389, for Remote Desktop access

Click to advance to the _Disks_ section - here you can create new virtual disks to attach to the VM.

## Create a Windows VM with the CLI

First we need to create a Resource Group where the new VM resources will live.

_Create the group - use your own preferred location:_

```
az group create -n labs-vm-win --tags courselabs=azure -l westeurope
```

Windows is a more demanding OS than Linux...

_Find a larger VM size we can use:_

```
# with PowerShell:
az vm list-sizes -o table --query "[?numberOfCores==``4`` && memoryInMb==``16384``]" --location "westeurope"

# or Bash:
az vm list-sizes -o table --query "[?numberOfCores==\`4\` && memoryInMb==\`16384\`]" --location "westeurope"
```

> The D series are the general-purpose machines, you should have an option like `Standard_D4s_v5`

OS images have a full name called a _URN_ which consists of:

- the _publisher_ (e.g. Microsoft or Canonical)
- the _offer_ (e.g. Ubuntu Server or Windows 11)
- the _SKU_ (e.g. Windows 11 Pro or Ubuntu Server LTS)
- the _version_ (the version number of the OS release)

To find the OS image to use, there's the `vm image list` command. You can filter with the `offer` option:

```
# show all the offers for Windows Desktop:
az vm image list-offers --publisher MicrosoftWindowsDesktop --location westeurope -o table

# show all the SKUs for Windows 11:
az vm image list-skus -l westus -f windows-11 -p MicrosoftWindowsDesktop -o table

# show all the Windows 11 Pro images:
az vm image list --sku win11-22h2-pro  -f windows-11 -p MicrosoftWindowsDesktop --location westeurope -o table --all
```

📋 Create an Windows 11 VM using a `vm create` command. Include a DNS name so you can access the machine without using the IP address.

<details>
  <summary>Not sure how?</summary>

The help text will get you to the DNS name parameter:

```
az vm create --help
```

Windows VMs need some more information - you need to specify:

- admin username
- admin password

This will get you started - you can use the exact version of the Windows 11 image, it will have a URN like this: _MicrosoftWindowsDesktop:windows-11:win11-22h2-pro:22621.674.221008_

Or - if you just want the most recent version - replace the version number with _latest_.

```
# your password will be verified - it needs to be strong:
az vm create -l westeurope -g labs-vm-win -n dev01 --image <image-urn> --size Standard_D4s_v5 --admin-username labs --public-ip-address-dns-name <your-unique-dns-name> --admin-password <your-strong-password>
```

</details><br/>

> Creating a Windows Desktop VM can take a little longer than a Linux Server VM. 

While it's running, open the Portal and check the resources created along with the VM:

- are they the same as for the Linux VM?
- where is the Remote Desktop port (3389) configured to allow access to the VM?

The supporting resources for the VM include the OS disk, which will be the C: drive in Windows.

## Add a data disk to the VM

When you delete a VM you typically delete the OS disk too, but if you want to retain data you can create another disk and attach it to the VM.

You manage the disks for a VM using `vm disk` commands:

```
az vm disk attach --help
```

📋 Add a new 2TB Premium disk to your Windows VM.

<details>
  <summary>Not sure how?</summary>

The `sku` parameter specifies the performance of the disk, the size needs to be set in GB, and the `new` flag creates the disk:

```
az vm disk attach -g labs-vm-win --vm-name dev01 --name dev01data --new --sku Premium_LRS --size-gb 2048
```

</details><br/>

> Premium storage uses fast solid-state disks in the data centre, so the performance is much better than standard disks.

Disks are charged separately from VMs, and large premium storage disks can be expensive. A deallocated VM with a premium disk attached doesn't incur compute costs for the VM but there are still storage costs for the disk.

## Connect and install dev tools

You can connect to your VM using a Remote Desktop client:

- on Windows - use the built-in Remote Desktop Connection app
- on Mac - install Microsoft Remote Desktop from the App Store
- on Linux - [Remmina](https://remmina.org) is a good option

Use your DNS name and admin credentials to connect to the VM. You'll launch into a Windows session with the final installation steps.

Copy the [setup.ps1](setup.ps1) PowerShell script to the VM (you can copy and paste from your own machine, or in the VM download it from [GitHub](https://raw.githubusercontent.com/courselabs/azure/main/labs/vm-win/setup.ps1)). Then run the script in an _Administrator_ PowerShell session. That installs Git and VS Code, so you'll be ready to work.

## Lab

Open Windows Explorer on your VM and check the machine setup. You'll see there's only one disk. But check in the Azure Portal and you'll see the second data disk attached to the VM. It's there but you'll need to configure the OS to initialize it.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG with this command and that will remove all the resources:

```
az group delete -y -n labs-vm-win
```