

## Create RG, VNet & subnet

```
az group create -n labs-vnet-access --tags courselabs=azure -l eastus

az network vnet create -g labs-vnet-access -n vnet1 --address-prefix "10.10.0.0/16"

az network vnet subnet create -g labs-vnet-access --vnet-name vnet1 -n subnet1 --address-prefix "10.10.1.0/24"
```


## Create NSG and VM

```

```

## Create second VNet and peer


## Lab