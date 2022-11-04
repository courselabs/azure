

We covered this in the [VNet lab](/labs/vnet/README.md) - any private IP address range will do:

```
az network vnet create -g labs-aci-compose -n appnet --address-prefix "10.10.0.0/16" -l eastus

az network vnet subnet create -g labs-aci-compose --vnet-name appnet -n aci --address-prefix "10.10.1.0/24"
```