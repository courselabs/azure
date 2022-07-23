# Virtual Machines - Windows

windows VM, as dev machine

## Reference

- [Azure Virtual Machine docs](https://docs.microsoft.com/en-gb/azure/virtual-machines/)

- [`az vm` commands](https://docs.microsoft.com/en-us/cli/azure/vm?view=azure-cli-latest)

- [`az vm image` commands](https://docs.microsoft.com/en-us/cli/azure/vm/image?view=azure-cli-latest)


## Explore Windows VMs in the Portal

Open the Portal and search to create a new Virtual Machine resource. . OS type = windows, what changes?

- authentication model
- incoming ports



## Create a Windows VM with the CLI

First we need to create a Resource Group where the new VM resources will live. This should be familiar from the [Resource Groups](/labs/resourcegroups/README.md) lab:

_Create the group - use your own preferred location:_

```
az group create -n labs-vm-win --tags courselabs=azure -l westeurope
```

_Find a larger VM size we can use:_

```
# with PowerShell:
az vm list-sizes -o table --query "[?numberOfCores==``4`` && memoryInMb==``16384``]" --location "westeurope"

# or Bash:
az vm list-sizes -o table --query "[?numberOfCores==\`4\` && memoryInMb==\`16384\`]" --location "westeurope"
```

> The D series are the general-purpose machines, you should have an option like `Standard_D4s_v5`

To find the OS image to use, there's the `vm image list` command. You can filter with the `offer` option:

- TODO, publisher, offer, urn

```
az vm image list-offers --publisher MicrosoftWindowsDesktop --location westeurope -o table

# this will take a while to run...
az vm image list --offer windows-11 --location westeurope -o table --all
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

This will get you started:

```
# your password will be verified - it needs to be strong:
az vm create -l westeurope -g labs-vm-win -n dev01 --image MicrosoftWindowsDesktop:windows-11:win11-21h2-pro:22000.795.220629 --size Standard_D4s_v5 --admin-username labs --public-ip-address-dns-name <your-unique-dns-name> --admin-password <your-strong-password>
```

</details><br/>

> Creating a new VM takes a few minutes. While it's running, check the docs to answer this:

- what is the running cost for your new VM?
- why is an "A" or "B" series VM not a good idea for normal workloads?

When your VM is created, browse to the portal and open the Resource Group. You'll see the VM together with all the supporting resources.

## Add a data disk to the VM

- retained when VM is deleted

You manage the disks for a VM using `vm disk` commands:

```
az vm disk attach --help
```

📋 Add a new 2TB Premium disk to your Windows VM.

<details>
  <summary>Not sure how?</summary>

The `sku` parameter specifies the performance of the disk, and the size needs to be set in GB:

```
az vm disk attach -g labs-vm-win --vm-name dev01 --name dev01data --new --sku Premium_LRS --size-gb 2048
```

</details><br/>

## Connect and install dev tools

You can connect to your VM using a Remote Desktop client:

- on Windows - use the built-in Remote Desktop Connection app
- on Mac - install Microsoft Remote Desktop from the App Store
- on Linux - [Remmina](https://remmina.org) is a good option

Use your DNS name and admin credentials to connect to the VM. You'll launch into a Windows session with the final installation steps.

Copy the [setup.ps1](setup.ps1) PowerShell script to the VM and run it in a PowerShell session. That installs Git and VS Code, so you'll be ready to work.

## Lab

Open Windows Explorer on your VM and check the machine setup. You'll see there's only one disk. But check in the Azure Portal and you'll see the second data disk attached to the VM. It's there but you'll need to configure the OS to initialize it.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG with this command and that will remove all the resources:

```
az group delete -y -n labs-vm-win
```