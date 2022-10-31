# Lab Solution

The default NSG rules allow traffic within a VNet to any port - and that applies to peered VNets too.

You can't delete the defaults, but you can create new rules with a higher priority, which will take precedence over the defaults:

- a rule to allow incoming traffic from IPs in the range 10.20.x.x on port 80
- a rule to deny all other incoming VNet traffic

And add new rules for VNets which are higher priority than the default:

```
# block all VNet access:
az network nsg rule create -g labs-vnet-access --nsg-name nsg01 -n 'BlockIncomingVnet' --direction Inbound --access Deny --priority 400 --source-address-prefixes 'VirtualNetwork' --destination-port-ranges '*'
  
# test from vm02 shell session - it will take a few minutes for the new rule to take effect, then this should fail:
curl --connect-timeout 2 <vm01-private-ip-address>
```

```
# allow access from 10.20 addresses:
az network nsg rule create -g labs-vnet-access --nsg-name nsg01 -n 'AllowSubnet2' --direction Inbound --access Allow --priority 300 --source-address-prefixes '10.20.0.0/16' --destination-port-ranges '80'

# test from vm02 shell session - when the rule is in place, this will work again:
curl --connect-timeout 2 <vm01-private-ip-address>
```