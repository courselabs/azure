# Virtual Machines - Web Server

VMs are a simple way to get a workload running in the cloud - something like a web server which needs to be available 24x7. Web servers have other requirements too, like a public IP address and a DNS name for access.

In this lab we'll see how to create a Linux VM and manually deploy a web server, by connecting to the VM and installing the packages we need.

## Reference

- [Azure public IP addresses](https://learn.microsoft.com/en-us/azure/virtual-network/ip-services/public-ip-addresses)

- [`az network public-ip` commands](https://learn.microsoft.com/en-us/cli/azure/network/public-ip?view=azure-cli-latest)

- [`az vm image` commands](https://docs.microsoft.com/en-us/cli/azure/vm/image?view=azure-cli-latest)


## Explore VM in the Portal

Open the Portal and search to create a new Virtual Machine resource. Under the _Networking_ tab you can specify the Public IP:

- you have to create a new resource for that, the Public IP Address (PIP)
- you can't choose an actual IP address though...
- you can also configure a network security group (NSG)
- you can set to delete the PIP and NSG when the VM is deleted

## Create a Linux VM with a public DNS name

First we need to create a Resource Group where the new VM resources will live. This should be familiar from the [Resource Groups](/labs/resourcegroups/README.md) lab:

_Create the group - use your own preferred location:_

```
az group create -n labs-vm-web --tags courselabs=azure -l westeurope
```

📋 Create an Ubuntu Server VM using the CLI. Specify a unique public DNS name to access the VM.  

<details>
  <summary>Not sure how?</summary>

Check in the help text:

```
az vm create --help
```

There's a parameter called `public-ip-address-dns-name` which you can use to set the DNS name:


```
# remember to use a size which is available to you:
az vm create -l westeurope -g labs-vm-web -n vm01 --image UbuntuLTS --size Standard_A1_v2 --public-ip-address-dns-name <your-dns-name>
```

</details><br/>

The DNS name is attached to the PIP (public IP address) which the VM uses. PIPs have their own lifecycle - you can manage them independently of any VMs using the CLI:

_List all the PIPs in the Resource Group:_

```
az network public-ip list -o table -g labs-vm-web
```

📋 Print the details of the PIP used by the VM to see the FQDN - fully qualified domain name.  

<details>
  <summary>Not sure how?</summary>

```
az network public-ip show -g labs-vm-web -n <your-pip-name>
```

</details><br/>

> The FQDN will be in the format `[vm-name].[region].cloudapp.azure.com`, e.g. mine is `courselabs-vm-web.westeurope.cloudapp.azure.com`

You can use the FQDN to connect to the VM - it will remain constant if the actual IP address changes.

## Install a web server on the VM

Connect to your VM using SSH and your DNS name:

```
ssh <your-fqdn>
```

Now install the Nginx web server:

```
sudo apt update && sudo apt install -y nginx

# when the installation completes, check you can browse:
curl localhost
```

> Open a web browser and navigate to the FQDN for your machine, http://[vm-name].[region].cloudapp.azure.com

You can access the website from the VM **but not** from outside - that's because of the Network Security Group (NSG). An NSG is created by default and attached to the VM. It's like a firewall which is set up to block incoming traffic.

When you're troubleshooting, the Portal can often be more useful than the CLI.

📋 Browse to the Portal and find the NSG for your VM. Change the configuration to allow inbound traffic on port 80.

<details>
  <summary>Not sure how?</summary>

Find your Resource Group in the portal and open the NSG - it will be called `[vm-name]NSG`:

- on the _Overview_ page you'll see the inbound rules
- port 22 is allowed (for SSH connections) and some 65000+ ports
- all other ports are blocked 
- open the _Inbound Security Rules_ page
- add a new rule to allow HTTP traffic from any source

</details><br/>

Refresh your browser at the FQDN for your VM and you'll see the Nginx welcome page.

## Stop and start the VM

You're billed for VMs all the time they're running. When you're finished working but you want to keep the VM for later, you can stop it - which retains the existing state.

**Stopped VMs are still billed though.** To stop paying for a VM you need to _deallocate_ it.

📋 Use the CLI to deallocate your VM. When it's done, check the details of the Public IP address the VM was using.

<details>
  <summary>Not sure how?</summary>

You can print all the available commands for a VM, then drill into the details for `stop`:

```
az vm --help

az vm deallocate --help
```

Run this to stop and deallocate the VM:

```
az vm deallocate -g labs-vm-web -n vm01
```

Then check your PIP:

```
az network public-ip show -g labs-vm-web -n vm01PublicIP
```

</details><br/>

> The IP address allocated to the PIP is gone - deallocating the VM also frees up the IP address for another user. 

If you restart the VM then you'll see the PIP gets a new public IP address, but you can still access your website using the FQDN, Azure sets up the DNS record to point to the new IP address.

## Lab

Dynamic IP addresses are usually fine - there will be a DNS entry to route them anyway - but sometimes you want to have a fixed IP address which you keep even if the VM is deallocated.

You can configure a PIP to use a fixed IP address, even if the PIP isn't allocated to a VM. Change the PIP to use a constant IP address and check the address is retained when you start, stop and deallocate the VM.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG with this command and that will remove all the resources:

```
az group delete -y -n labs-vm-web
```