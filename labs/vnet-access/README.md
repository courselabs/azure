

## Create RG, VNet & subnet

```
az group create -n labs-vnet-access --tags courselabs=azure -l eastus

az network vnet create -g labs-vnet-access -n vnet1 --address-prefix "10.10.0.0/16"

az network vnet subnet create -g labs-vnet-access --vnet-name vnet1 -n subnet1 --address-prefix "10.10.1.0/24"
```

## Create NSG and VM

```
az network nsg create -g labs-vnet-access -n nsg01
```

Check in Portal - default rules applied:

- allow incoming from VNet and Azure LB
- deny all incoming backup rule
- allow outgoing to vnet and internet
- default deny all outgoing

Also check the location - it uses your az default, not the region set in the RG.

** If your NSG is in a different region from your VNet then they can't be associated. You'll need to create a new NSG in the same region as the VNet:

```  
az network nsg delete -g labs-vnet-access -n nsg01

az network nsg create -g labs-vnet-access -n nsg01 -l <region>
```

Add specific rules to block external traffic except on port 80:

```
  az network nsg rule create -g labs-vnet-access --nsg-name nsg01 -n 'BlockIncoming' --direction Inbound --access Deny --priority 200 --source-address-prefixes 'Internet' --destination-port-ranges '*'
  
  az network nsg rule create -g labs-vnet-access --nsg-name nsg01 -n 'AllowHttp' --direction Inbound --access Allow --priority 100 --source-address-prefixes 'Internet' --destination-port-ranges '80'
```

Apply the NSG to the subnet:

```
az network vnet subnet update -g labs-vnet-access  --vnet-name vnet1  --name subnet1 --network-security-group nsg01
```

Check subnet in the Portal


## Connect with Bastion


Create a VM - we'll use password auth for this one:

```
az vm create -g labs-vnet-access -n ubuntu01 --image UbuntuLTS --vnet-name vnet1 --subnet subnet1 --nsg nsg01 --admin-username labs --admin-password <strong-password> -l <region>
```

Check in the Portal - the NSG is applied to the VM.

Try to connect to the machine directly:

```
# this will time out - SSH uses port 22 which is blocked by the NSG rules:
ssh labs@<publicIpAddress>
```

Azure has [Bastion]() for accessing VMs which are in locked-down networks:

- open the VM in the Portal
- click _Connect_ and choose _Bastion_ from the dropdown
- Choose _Create Azure Bastion using defaults_

This will take a few minutes.

Enter your VM username and password - a browser window will open with a terminal connection to the VM.

Install the Nginx web server:

```
sudo apt update && sudo apt install -y nginx
```

Browse to your VM's public IP address and verify traffic is allowed through the NSG on port 80:

> http://<publicIpAdress>

## Create second VNet and peer

Non-overlapping CIDR block - can be in a different region from vnet1:

```
az network vnet create -g labs-vnet-access -n vnet2 --address-prefix "10.20.0.0/16" -l <region2>

az network vnet subnet create -g labs-vnet-access --vnet-name vnet2 -n subnet2 --address-prefix "10.20.1.0/24"
```

New VM - SSH for this one:

```
az vm create -g labs-vnet-access -n remote01 --image UbuntuLTS --vnet-name vnet2 --subnet subnet2 -l <region2>
```

Connect to the new VM and verify the first VM is unreachable on its private IP address:

```
ssh <publicIpAddress>

# this will time out
curl <vm01-private-ip-address>
```

Now in a separate terminal, peer the VNets:

```
az network vnet peering create -g labs-vnet-access -n vnet2to1 --vnet-name vnet2 --remote-vnet vnet1 --allow-vnet-access
```

Open the vnet in the portal - under _Peerings_ you might see a message that resync is required - you can start that in the portal. When it completes you'll see the address space of the remote VNet listed (`10.10.0.0/16`).

The VNets are peered so VMs in subnet2 (addresses starting 10.20) can reach VMs in subnet1 (addresses starting 10.10).

In the SSH session for your second VM try accessing the first VM again:

```
# now this works:
curl <vm01-private-ip-address>

# check your IP addresses - you'll only see a 10.20 address:
ip a 
```

> Peering doesn't add a new NIC to the VM, it takes care of routing between the networks.


## Lab

Update the NSG rules so that traffic is only allowed to the web server from two origins:

- your own machine on the public Internet
- Azure VMs in the subnet2 address space