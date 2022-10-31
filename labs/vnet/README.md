# Virtual Networks

Virtual Networks are private to Azure - services can communicate with each other in a Virtual Network (vnet) without being accessible on the public Internet. 

Vnets are a core component in deploying secure solutions in Azure, and you should aim to use them in all your application, provided the services you're using support them. You create the vnet first and deploy other services into it. You can't typically move resources between vnets, so you need to plan your networking up front.

## Reference

- [Virtual Network overview](https://docs.microsoft.com/en-gb/azure/virtual-network/)

- [`az network` commands](https://docs.microsoft.com/en-us/cli/azure/network?view=azure-cli-latest)

- [`az network vnet` commands](https://docs.microsoft.com/en-us/cli/azure/network/vnet?view=azure-cli-latest)

- [`az network vnet subnet` commands](https://docs.microsoft.com/en-us/cli/azure/network/vnet/subnet?view=azure-cli-latest)

## Explore Virtual Networks in the Portal

Open the Portal and search to create a new Virtual Network resource. There aren't as many options as other services:

- the name doesn't need to be globally unique, just unique within the RG
- IP addresses - you need to select an address range for the whole vnet (from a [private CIDR range](https://en.wikipedia.org/wiki/Private_network#Private_IPv4_addresses))
- every vnet needs at least one subnet, and subnets have their own IP range within the vnet range
- you can create multiple subnets to isolate workloads in a single vnet

Back to the CLI to create a vnet and some services inside it.

## Create a Virtual Network with the CLI

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-vnet --tags courselabs=azure -l eastus
```

📋 Create a new vnet with the `network vnet create` command. Call it `vnet1` and use the address space `10.10.0.0/16`.

<details>
  <summary>Not sure how?</summary>

Start with the help:

```
az network vnet create --help
```

You need to specify the RG, vnet name and address prefix:

```
az network vnet create -g labs-vnet -n vnet1 --address-prefix "10.10.0.0/16"
```

</details><br/>

When you create a vnet in the Portal, it has a subnet created by default. A basic `vnet create` command doesn't give you a subnet:

```
az network vnet show -g labs-vnet -n vnet1
```

Subnets are where you actually deploy services, so you need at least one in your vnet.

📋 Create two subnets in the vnet with `vnet subnet create `. Use the names `frontend` and `backend` and the address ranges `10.10.1.0/24` and `10.10.2.0/24`.

<details>
  <summary>Not sure how?</summary>

Subnets have their own help text:

```
az network vnet subnet create --help
```

You need to specify the RG, vnet, subnet name and address range:

```
az network vnet subnet create -g labs-vnet --vnet-name vnet1 -n frontend --address-prefix "10.10.1.0/24"

az network vnet subnet create -g labs-vnet --vnet-name vnet1 -n backend --address-prefix "10.10.2.0/24"
```

</details><br/>

> You can't have overlapping IP address ranges in a subnet, or use a range which isn't in the parent vnet. Does the CLI enforce that?

## Create a Virtual Machine in the VNet

We covered Virtual Machines in the [VM lab](/labs/vm/README.md) - they're an IaaS approach and there's usually a better way of running apps in Azure. But they're easy to work with and we can use them to check the networking in the vnet.

Create a Linux VM running Ubuntu Server:

```
az vm create -g labs-vnet -n vm01 --image UbuntuLTS --vnet-name vnet1 --subnet frontend --generate-ssh-keys
```

This command takes care of setting up [SSH](https://en.wikipedia.org/wiki/Secure_Shell) so you can log into the remote machine. The output shows you the public IP address you'll use to connect.

📋 If you wanted to create a Windows VM, you'd need to use a different image. Can you find a Windows Server 2019 image name with an `az` command?

<details>
  <summary>Not sure how?</summary>

Use the `az vm image` commands to work with available VM images:

```
az vm image list --help
```

Listing all images will take a while, so you can filter the OS name using the `offer` parameter:

```
az vm image list --offer  Windows -o table
```

You'll see lots of images with long names - but you can use the alias in the `vm create` command. The Windows Server 2019 image is called `Win2019Datacenter`.

</details><br/>

## Connect to the VM

It shouldn't take too long for the `vm create` command to complete. When it does your Linux VM is ready to use.

📋 Use an `az vm` command to print the public IP address of `vm01`.

<details>
  <summary>Not sure how?</summary>

The `show` command prints the basic information about a resource:

```
az vm show -g labs-vnet -n vm01
```

You'll see lots of data, but not the public IP address. Run `az vm show --help` and you'll see there's a `--show-details` option. You can use that with a query to print just the public IP address:

```
az vm show -g labs-vnet -n vm01 --show-details --query publicIps -o tsv
```

</details><br/>

Now you can use `ssh` to connect to the VM (it's installed by default in macOS, Linux and Windows 10+):

```
ssh <vm01-public-ip>

# now in the VM session:
ip address

# exit back to your own machine:
exit
```

VMs only know about their local IP address on the vnet. The public IP address is managed otuside of the machine; the private IP is assigned by the vnet and you should see it in the `10.10.1.x` range.

## Explore Networking in the Portal

We've created a few resources with `az` commands. Open your Resource Group in the Portal - it shows objects we didn't explicitly create:

- a disk which is the virtual storage unit attached to the VM
- a NIC which connects the VM to the vnet
- a Network Security Group which controls network access to the VM
- a Public IP Address

> Click on _Resource Visualizer_ to see how all the resources are related.

They were all created with default configuration, but you can create and manage them independently with `az` commands if you need more control.

## Lab

The `az` command is a great tool, but it has one drawback: it's an _imperative_ approach. When you create resources you tell Azure what to do, which gets difficult if you need to re-run scripts because you'll get errors about resources already existing.

Azure also supports a _declarative_ approach where you describe what the end result should be. These are [Azure Resource Manager (ARM) templates](https://docs.microsoft.com/en-us/azure/azure-resource-manager/templates/) - and you can run them repeatedly and always get the same result.

Try exporting an ARM template for the `labs-vnet` Resource Group. Can you use it to deploy a copy of the resources in a new RG called `labs-vnet2`?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RGs for this lab to remove all the resources:

```
az group delete -y -n labs-vnet

az group delete -y -n labs-vnet2
```