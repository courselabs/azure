# Virtual Machine Scale Sets - Linux

With cloud-init. Simpler to configure and manage - good where install reqs can be deployed quickly so new instances come online fast.

## Reference

- [Azure cloud-init support for Linux VMs](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/using-cloud-init)

- [cloud-init config examples](https://cloudinit.readthedocs.io/en/latest/topics/examples.html#)

## Create a VM with cloud-init

```
az group create -n labs-vmss-linux --tags courselabs=azure -l westeurope
```

- [cloud-init.txt](labs/vmss-linux/setup/cloud-init.txt)

```
# remember to use a size which is available to you:
az vm create -l westeurope -g labs-vmss-linux -n web01 --image UbuntuLTS --size Standard_A1_v2 --custom-data @labs/vmss-linux/setup/cloud-init.txt --public-ip-address-dns-name labs-vmss-linux-es #<your-dns-name>
```


az vm run-command invoke  -g labs-vmss-linux -n web03 --command-id RunShellScript --scripts "cat /var/log/cloud-init-output.log"


> Will see install log for Nginx

az vm run-command invoke  -g labs-vmss-linux -n web01 --command-id RunShellScript --scripts "curl localhost"


## Use cloud-init for Linux VMSS



az vmss create -n vmss-web01 -g labs-vmss-linux --vm-sku Standard_D2s_v5 --instance-count 3 --image UbuntuLTS --custom-data @labs/vmss-linux/setup/cloud-init-custom.txt --public-ip-address-dns-name <unique-dns-name>

Create LB health probe & routing rule:

az network lb list -g labs-vmss-linux -o table

az network lb probe create -g labs-vmss-linux -n 'http' --protocol tcp --port 80  --lb-name vmss-web01LB #<lb-name> 


az network lb rule create -g labs-vmss-linux --probe-name 'http' -n 'http' --protocol Tcp --frontend-port 80 --backend-port 80 --lb-name vmss-web01LB #<lb-name> 
             

> Browse to check output; curl to check LB

## Update VMSS 

show model:

az vmss show -g labs-vmss-linux -n vmss-web01

update custom data - failes, needs to be base64:

az vmss update -g labs-vmss-linux -n vmss-web01 --set virtualMachineProfile.osProfile.customData=@labs/vmss-linux/setup/cloud-init-updated.txt

update with base64:

$customData=$(cat labs/vmss-linux/setup/cloud-init-updated.txt | base64)

az vmss update -g labs-vmss-linux -n vmss-web01 --set virtualMachineProfile.osProfile.customData=$customData

> Check instancesin portal - VMs show "no" in latest model but do not get reprovisioned]

rollout the update:

az vmss update-instances  -g labs-vmss-linux -n vmss-web01 --instance-ids '*' 

chec instances again - all latest; refresh - not changed; custom data only processed at provisining time

scale up - when the new instances come online, they will have new content, check with curl:

az vmss scale -g labs-vmss-linux -n vmss-web01 --new-capacity 5

## Lab

configure vmss to auto-update and deploy changed init script

