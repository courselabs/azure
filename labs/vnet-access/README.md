# Securing VNet Access

Virtual Networks are great for restricting traffic to services in Azure, and they give you a lot of options for securing access to resources. The Network Security Group is the main mechanism, where you can define rules allowing or denying traffic from specific sources and to specific ports. 

You can also join VNets together if you need different parts of an application to access each other, and use Bastion to access VMs which are in networks that don't allow public access.

## Reference

- [Network Security Groups](https://learn.microsoft.com/en-us/azure/virtual-network/network-security-groups-overview)

- [VNet peering](https://learn.microsoft.com/en-us/azure/virtual-network/virtual-network-peering-overview)

- [Bastion](https://learn.microsoft.com/en-gb/azure/bastion/bastion-overview)

- [`az network nsg` commands](https://learn.microsoft.com/en-us/cli/azure/network/nsg?view=azure-cli-latest)

## Create a VM and an NSG

Start by creating a Resource Group, VNet and subnet:

```
az group create -n labs-vnet-access --tags courselabs=azure -l eastus

az network vnet create -g labs-vnet-access -n vnet1 --address-prefix "10.10.0.0/16"

az network vnet subnet create -g labs-vnet-access --vnet-name vnet1 -n subnet1 --address-prefix "10.10.1.0/24"
```

📋 Create a Network Security Group using a `network` command in the CLI.

<details>
  <summary>Not sure how?</summary>

Check the help - this prints out the subgroups for network objects:

```
az network --help
```

`nsg` is the group to use:

```
az network nsg create --help
```

It just needs a name and an RG:

```
az network nsg create -g labs-vnet-access -n nsg01
```

</details>

Open the NSG in the Portal - there are default rules applied to all new NSGs:

- allow incoming from VNet and Azure LB
- deny all other incoming
- allow outgoing to VNet and internet
- default deny all outgoing

Also check the location - if you didn't set the locations in the commands, your VNet and NSG may be in different regions.

**If your NSG is in a different region from your VNet then they can't be associated. You'll need to create a new NSG in the same region as the VNet**:

```  
az network nsg delete -g labs-vnet-access -n nsg01

az network nsg create -g labs-vnet-access -n nsg01 -l <region>
```

Add a new rule to allow incoming traffic from the internet on port 80:

```
az network nsg rule create -g labs-vnet-access --nsg-name nsg01 -n 'AllowHttp' --direction Inbound --access Allow --priority 100 --source-address-prefixes 'Internet' --destination-port-ranges '80'
```

📋 Use the CLI to attach the NSG to the subnet - the NSG is a property of the subnet itself.

<details>
  <summary>Not sure how?</summary>

We're looking to update the subnet:

```
az network vnet subnet update --help
```

We can set the NSG by name:

```
az network vnet subnet update -g labs-vnet-access  --vnet-name vnet1  --name subnet1 --network-security-group nsg01
```

</details>

Open the VNet in the Portal and check the subnet - you can confirm that the NSG is attached here. Any services deployed into the VNet are subject to these NSG rules now.

That means port 22 (SSH) and 3389 (RDP) are blocked, so if we had VMs running in this VNet we couldn't access them any more. We'll need to use another service to do that.

## Connect with Bastion

Create a basic Linux VM - we'll use password authentication this time instead of the default SSH key:

```
# be sure to use the same location as the VNet:
az vm create -g labs-vnet-access -n ubuntu01 --image UbuntuLTS --vnet-name vnet1 --subnet subnet1 --admin-username labs --admin-password <strong-password> -l <region>
```

Check the VM in the Portal - you'll see the NSG listed in the _Networking_ tab even though we didn't explicitly set in when we created the VM.

Try to connect to the machine:

```
# this will time out - SSH uses port 22 which is blocked by the NSG rules:
ssh labs@<publicIpAddress>
```

Azure has [Bastion](https://learn.microsoft.com/en-gb/azure/bastion/bastion-overview) for accessing VMs which are in locked-down networks:

- open the VM in the Portal
- click _Connect_ and choose _Bastion_ from the dropdown
- Choose _Create Azure Bastion using defaults_

This will take a few minutes. The Bastion service is created at the VNet level, and the same Bastion instance can be used for all the VMs in the VNet.

When the Bastion setup completes, enter `labs` as the username and the password you used to create the VM and click _Connect_. A browser window will open with a terminal connection to the VM, but port 22 is still blocked for direct access.

In your VM session, install the Nginx web server:

```
sudo apt update && sudo apt install -y nginx
```

Browse to your VM's public IP address and verify traffic is allowed through the NSG on port 80:

> `http://<publicIpAdress>`

## Create second VNet and peer

VNets are a good way of isolating parts of an application, but sometimes you want components in one VNet to be able to reach components in a different VNet. Maybe you have VNets in different regions hosting different services. You can connect those two VNets together in Azure using _peering_.

📋 Create a new VNet with the IP address range `10.20.0.0/16` in a different region from the first VNet, and a new subnet with the range `10.20.1.0/24`.

<details>
  <summary>Not sure how?</summary>

```
az network vnet create -g labs-vnet-access -n vnet2 --address-prefix "10.20.0.0/16" -l <region2>

az network vnet subnet create -g labs-vnet-access --vnet-name vnet2 -n subnet2 --address-prefix "10.20.1.0/24"
```

</details>

> You need to plan your networking in advance. If you want to peer two VNets they need to have non-overlapping IP address ranges.

Create a new VM, attached to the new VNet which has no NSG. Azure will create a new NSG for the VM, with an additional default rule to allow incoming SSH traffic:

```
az vm create -g labs-vnet-access -n remote01 --image UbuntuLTS --vnet-name vnet2 --subnet subnet2 -l <region2>
```

Print the private IP addresses of the two VMs:

```
az vm list -g labs-vnet-access --show-details --query "[].{VM:name, InternalIP:privateIps, PublicIP:publicIps}" -o table
```

Connect to the new VM and check if you can reach the web browser on the first VM using the private IP address (10.10.1.x):

```
# this will connect because the VM's NSG allows port 22:
ssh <vm02-public-ip-address>

# this will time out:
curl <vm01-private-ip-address>
```

Now in a separate terminal, peer the VNets - you need to peer both networks (this ensures you can't peer onto someone else's VNet that you don't have access to):

```
az network vnet peering create -g labs-vnet-access -n vnet2to1 --vnet-name vnet2 --remote-vnet vnet1 --allow-vnet-access

az network vnet peering create -g labs-vnet-access -n vnet1to2 --vnet-name vnet1 --remote-vnet vnet2 --allow-vnet-access
```

Open the new VNet in the portal - under _Peerings_ you should see that the VNets are peered with the status _Connected_. Now VMs in subnet2 (addresses starting 10.20) can reach VMs in subnet1 (addresses starting 10.10).

In the SSH session for your second VM try accessing the first VM again:

```
# now this works:
curl <vm01-private-ip-address>

# check your IP addresses - you'll only see a 10.20 address:
ip a 
```

> Peering doesn't add a new NIC to the VM, it takes care of routing between the networks.


## Lab

Now the web server in subnet1 is accessible from any machine on the Internet, and VMs in subnet2. But those VMs in subnet2 can access any port on the VM in subnet1, including SSH - which we don't want. Update the NSG rules so that traffic is only allowed to the web server from subnet2 machines on port 80.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG with this command and that will remove all the resources:

```
az group delete -y --no-wait -n labs-vnet-access
```