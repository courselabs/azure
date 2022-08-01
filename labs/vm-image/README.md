# Building Custom VM Images


## Reference

- linux - https://docs.microsoft.com/en-gb/azure/virtual-machines/linux/imaging

- windows - https://docs.microsoft.com/en-gb/azure/virtual-machines/windows/prepare-for-upload-vhd-image

- builder - https://docs.microsoft.com/en-gb/azure/virtual-machines/image-builder-overview?tabs=azure-powershell


## Create a Base VM

az group create -n labs-vm-image --location westeurope

az vm image list-skus -l westus -p MicrosoftWindowsServer -f WindowsServer -o table

> Look for a 2022 datacenter core SKU

az vm create -l westeurope -g labs-vm-image -n app01-base --image MicrosoftWindowsServer:WindowsServer:2022-datacenter-core-g2:latest --size Standard_D2s_v5 --admin-username labs --public-ip-address-dns-name <your-unique-dns-name> --admin-password <your-strong-password>


## Install Application

- iis  & asp.net
- download app/default.aspx

RDP

```
Install-WindowsFeature Web-Server,NET-Framework-45-ASPNET,Web-Asp-Net45

# TODO - fix URL:
curl -o C:/inetpub/wwwroot/default.aspx https://raw.githubusercontent.com/sixeyed/docker4.net/master/docker/01-05-dockerfiles-and-images/hostname-app/default.aspx

rm -fo C:\inetpub\wwwroot\iisstart.htm

curl.exe http://localhost
```

Run sysprep - tool which generalizes the OS to make other vms from it

```
C:\windows\system32\sysprep\sysprep.exe
```

- select _Enter System Out-of-Box Experience (OOBE)_, tick _Generalize_ & shutdown

> Not accessible externally - NSG - but can create image now

## Create an Image from the VM

az vm deallocate -g labs-vm-image -n app01-base

doc - generalized vs. specialized, removing or retaining user accounts

# mark so AZ knows the vm is generalized

az vm generalize -g labs-vm-image -n app01-base

az vm show --show-details -g labs-vm-image -n app01-base

> Power state _VM deallocated_ & no public IP

az image create -g labs-vm-image -n app01-image --source app01-base --hyper-v-generation V2

az image list -o table

> Check in the portal - option to create a VM or clone to a new image

## Create Multiple VMs from the Image

az vm create -l westeurope -g labs-vm-image -n app-n --image app01-image --size Standard_D2s_v5 --admin-username labs  --count 3 --admin-password 'djkfUHUhgf77**' # pwd


## Lab

enable nsg port 80 - get the vm ip addresses, check each one. fine but individual; load balance with traffic manager - create with portal

