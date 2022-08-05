# Virtual Machine Scale Sets - Windows

From image - good for Windows apps which need custom setup, or where the install takes too long - new instance needs to be pre-provisioned.

## Portal

- search vmss, select Virtual Machine Scale Sets
- usual VM stuff - image, size, disks
- _Scaling_ section - number of VMs, scaling policy - switch to _Custom_, "scale out" and "scale in" thresholds and instance count


## Create a VMSS from a Custom Image

You should have your RG created with an image ready to go from the [VM images exercises](/labs/vm-image/README.md):


az group show -o table -n labs-images #labs-vmss-win

az image list  -o table -g labs-images #labs-vmss-win


az vmss create -n vmss-app01 -g labs-images --vm-sku Standard_D2s_v5 --instance-count 3 --backend-port 3389 --image app01-image-westeurope --admin-username labs --admin-password ''


az vm list -o table  -g labs-images 

> No VMs, VMSS take care of management

Switch to the portal:

- open the rg - you have a VMSS, LB, VNET and Pip

- open the vmss and check the _Instances_ blade - you'll see three instances, they may be in different statuses (Running, Updating)

- click on one of the VMSS instances - it has a private ip address but no public one

- back in the RG open the PIp; you'll see it's associated to the LB. 

> browse to the public IP address - does the app respond?

## Load Balancer Configuration

Creating the VMSS also sets up the load-balancer, which is a routing component. It listens on the Public IP address and routes traffic between the VMSS instances.

Why doesn't the app work? Because the VMSS setup doesn't include any load balancing rules - so the LB has no routing table to use.

Open the LB in the portal:

- select _Backend pools_ and confirm the VMSS instances are all there & running
- select _Health probes_ - this is how the LB checks that resources in the backend pool are ready to receive traffic; no probes so far
- select _Load balangcing rules_ - none, which is why the app doesn't work, the LB receives the req but has no rules to fetch from the backend

Add a rule to listen on the frontend PIP and route to the VMSS backend pool, using port 80 

- also need to add a health probe; type http will check response status code

Browse to your PIP again - now you'll see the app. If you refresh do you get responses from different VMs?

## Scaling VMSS

There's lots of caching in the browser stack, try using the command line to make a GET request to the PIP:

```
curl http://<pip>
```

Repeat and you should see different VM names in the response. 

Scale up to five instances using the `vmss scale` command. How quickly do they come on line and start serving responses?

```
az vmss scale -g labs-images -n vmss-app01 --new-capacity 5
```

Check in the portal - see new instances listed in VMSS blade, and they will get added to the LB backed pool too. When they are healthy, they'll become valid targets for the LB.

> Will be creating for a few  minutes; Windows VMs are not as fast to commission as Linux


You may also see more than 5 - why? VMSS overprovisions - it knows there is variation in VM startup time, so it creates more than you need and when the desired count are online, it removes the extras.

We're using manual scale, you can also set autoscale:

- open the vmss in the portal and the _Scaling_ blade
- switch to _Custom autoscale_ 
- select _Scale based on a metric_
- set minimum 2, max 2, default 2
- add a rule to scale out - increase by one instance if avg cpu > 10%
- add a rule to scale in - decrease by two instances if avg cpu > 8%
- set small timescales to see changes quickly (e.g. 2m)

What happens to the instance count? Does the app still work when a scaling event is in progress?

You should see two deleting; those are removed from the LB. After a few minutes with no activity, another 1 is removed bringing to minimum 2.

## Lab 
 
Triggering health probe failures - log into one instance, stop the IIS service, does it get any more requests? Can you see the probe status in the portal?