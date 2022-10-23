# Lab Solution

The process we want is called _reimaging_ for the VMSS instances. You can select instances in the Portal and reimage them, or use the command line.

You could blanket reimage all the VMs, but the most recent ones are in the correct state. Instead you need to find the instance IDs of the original VMs and reimage them individually:

```
# in my case, the original VMs were ID 0, 2 and 3:
az vmss reimage --instance-id 0 -g labs-vmss-linux -n vmss-web01

az vmss reimage --instance-id 2 -g labs-vmss-linux -n vmss-web01

az vmss reimage --instance-id 3 -g labs-vmss-linux -n vmss-web01
```

In the Portal you will see the state of the instances change to _Updating_ - while that's happening the instances will be taken out of the load balancer so you should only see responses from the new VMs until the reimaged ones come back online.

> Back to the [exercises](README.md).