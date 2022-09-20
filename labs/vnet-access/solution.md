

You can restrict access from the Internet to a specific IP address.

Grab your external IP address:

```
curl ifconfig.me
```

Update the NSG Internet rule (you can also do this in the Portal)

```
  az network nsg rule update -g labs-vnet-access --nsg-name nsg01 -n 'AllowHttp' --source-address-prefixes 82.132.212.32 #<your-ip-address> -

# test access:
curl <vm01-publicIpAddress>
```

And add new rules for VNets which are higher priority than the default:

```
# block all VNet access:
  az network nsg rule create -g labs-vnet-access --nsg-name nsg01 -n 'BlockIncomingVnet' --direction Inbound --access Deny --priority 400 --source-address-prefixes 'VirtualNetwork' --destination-port-ranges '*'
  
# test from vm02 shell session - it will take a few minutes for the new rule to take effect:
curl <vm01-private-ip-address>
```

```
# allow access from 10.20 addresses:
  az network nsg rule create -g labs-vnet-access --nsg-name nsg01 -n 'AllowSubnet2' --direction Inbound --access Allow --priority 300 --source-address-prefixes '10.20.0.0/16' --destination-port-ranges '80'

  # test from vm02 shell session:
curl <vm01-private-ip-address>
```