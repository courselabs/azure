# Virtual Machines

Virtual Machines in the cloud are pretty much the same as VMs in the datacenter or on your desktop. They're an isolated compute environment running with a full operating system, where you have admin permissions to install and configure whatever you need. Azure can run Linux and Windows VMs, with a large choice of preconfigured images and compute sizes.

## Reference

- [Azure Virtual Machine docs](https://docs.microsoft.com/en-gb/azure/virtual-machines/)

- [`az vm` commands](https://docs.microsoft.com/en-us/cli/azure/vm?view=azure-cli-latest)

- [`az vm image` commands](https://docs.microsoft.com/en-us/cli/azure/vm/image?view=azure-cli-latest)


## Explore VMs in the Portal

Open the Portal and search to create a new Virtual Machine resource. There are lots of configuration options but the main ones are:

- image - what does the VM image give you?
- size - how does compute capacity affect cost?
- authentication and inbound ports - how will you connect to your VM?

The basic options cover the OS type, CPU, memory and connectivity. Look at the required options - what other resources do you need to create before you get to the VM?

> All resources belong inside a resource group; you can typically create dependent resources directly in the portal.

Check the _Disks_ and _Networking_ tabs and you'll see how you can configure VMs with the exact setup you need:

- you can add multiple disks to the VM. What are the performance differences with disk types?
- you can configure network access at the port level. What type of object can you create to enforce those rules?

We won't go on to create the VM in the portal, we'll use the CLI instead.

## Create a Linux VM with the CLI

First we need to create a Resource Group where the new VM resources will live. This should be familiar from the [Resource Groups](/labs/resourcegroups/README.md) lab:

_Create the group - use your own preferred location:_

```
az group create -n labs-vm --tags courselabs=azure -l westeurope
```

_Find a valid (small) VM size for your subscription & region:_

```
# with PowerShell:
az vm list-sizes -o table --query "[?numberOfCores<=``2`` && memoryInMb==``2048``]" --location "westeurope"

# or Bash:
az vm list-sizes -o table --query "[?numberOfCores<=\`2\` && memoryInMb==\`2048\`]" --location "westeurope"
```

> JMESPath takes some getting used to. How are we filtering the list of VMs?

The VM sizes available will depend on your subscription, the region you choose and the spare capacity in that region. The Azure free trial subscriptions might have restrictions which paid subscriptions don't. 

Now you can create a small VM which will be cheap to run.

📋 Create an Ubuntu Server VM using a `vm create` command. There are a few parameters you'll need to specify.

<details>
  <summary>    
    Not sure how?  
  </summary>
    
Print the help text:

```
az vm create --help
```

As a minimum you need to specify:

- resource group
- location
- VM name 
- OS image

This will get you started:

```
# it's good to include a size, as the default might not be available
az vm create -l westeurope -g labs-vm -n vm01 --image UbuntuLTS --size Standard_A1_v2
```

</details><br/>

> Creating a new VM takes a few minutes. While it's running, check the docs to answer this:

- what is the running cost for your new VM?
- why is an "A" or "B" series VM not a good idea for normal workloads?

When your VM is created, browse to the portal and open the Resource Group. You'll see the VM together with all the supporting resources.

## Connect to the VM

This is a Linux VM, so you can use [SSH]() to connect - the SSH command line is installed by default on MacOS, Linux and the latest Windows machines.

📋 Find the IP address of your server and connect with `ssh`. 

<details>
  <summary>
    Not sure how?
  </summary>

The key details of the VM are printed when the `vm create` command completes. You can print them again with the `vm show` command:

```
az vm show --help
```

You'll see there's a parameter to set if you want to include the IP address in the response:

```
az vm show -g labs-vm -n vm01 --show-details
```

The field you want is `publicIps`. You can add a query to return just that field and store the IP address in a variable:

```
# using PowerShell:
$pip=$(az vm show -g labs-vm -n vm01 --show-details --query "publicIps" -o tsv)

# or a Linux shell:
pip=$(az vm show -g labs-vm -n vm01 --show-details --query "publicIps" -o tsv)
```

(Or you can find the public IP address from the Portal).

Now you can connect:

```
ssh $pip
```
</details><br/>

You should be able to connect without specifying a username or password. 

This is a standard Ubuntu Server VM. You can run typical commands like:

- `top` to see the processes running
- `uname -a` to see the details of the Linux build
- `curl https://azure.courselabs.co` to make an HTTP request
- `exit` to leave the SSH session

## Lab

Use the CLI to print the details of the VM's disk. What is the disk performance in read/write IOPS? Then delete the VM - does the disk get deleted too?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG with this command and that will remove all the resources:

```
az group delete -y -n labs-vm
```