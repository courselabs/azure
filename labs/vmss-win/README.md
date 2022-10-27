# Virtual Machine Scale Sets - Windows

You can create multiple VMs from the same image and have multiple instances of your application - but they're all separate VMs with their own IP addresses and PIPs. When you need multiple VM instances, you can use a Virtual Machine Scale Set (VMSS), which let you manage several VMs from a single resource.

In this lab we'll use an existing application image and create a VMSS for a Windows application.

## Reference 

- [Virtual Machine Scale Sets overview](https://learn.microsoft.com/en-gb/azure/virtual-machine-scale-sets/overview)

- [Use a custom image to create a VMSS](https://learn.microsoft.com/en-gb/azure/virtual-machine-scale-sets/tutorial-use-custom-image-cli)

- [`az vmss` commands](https://learn.microsoft.com/en-us/cli/azure/vmss?view=azure-cli-latest)

## Portal

Start by exploring in the Portal - create a new resource and search for VMSS. Select to create the Virtual Machine Scale Set and look through the options:

- there's the usual VM settings - image, size and disks
- you can choose an [Orchestration mode](https://learn.microsoft.com/en-gb/azure/virtual-machine-scale-sets/virtual-machine-scale-sets-orchestration-modes) - uniform is the most common
- under the _Scaling_ section you select the number of VM instance in the scale set; change the scaling policy to _Autoscaling_ and you'll see "scale out" and "scale in" thresholds and instance counts


## Create a VMSS from a Custom Image

You should have your RG called `labs-vmss-win` created with an image ready to go from the [VM images exercises](/labs/vm-image/README.md):

```
az group show -o table -n labs-vmss-win

az image list -o table -g labs-vmss-win
```

> This should show the `app01-image` you created and moved (or copied) to the new RG. If not you'll need to repeat those steps of the VM image lab.

📋 Use the `vmss create` command to create a scale set from your VM image, with three instances. Make the RDP port 3389 available so you can connect to the VMs.

<details>
  <summary>Not sure how?</summary>

Check the command help:

```
az vmss create --help
```

You need to specify the VM SKU, instance count, backend port, image and admin credentials:

```
# choose your own VM size and location:
az vmss create -n vmss-app01 -g labs-vmss-win --vm-sku Standard_D2s_v5 --instance-count 3 --backend-port 3389 --image app01-image --admin-username labs --admin-password '<strong-password>' -l westeurope
```

</details><br/>

When your VMSS is created, check to see the VM list in the RG:

```
az vm list -o table -g labs-vmss-win 
```

> There are no individual VMs, you manage instances through the VMSS

Open the VMSS in the portal:

- open the RG - what resources do you have now, besides the image?

- open the vmss and check the _Instances_ blade - you'll see three instances, they may be in different statuses (Running, Updating) and they may not have sequential numbers

- click on one of the VMSS instances - it has a private IP address but no public one

- back in the RG you'll see there's a public IP address - open that and it shows it's associated to a _load balancer_

> Browse to the public IP address - does the app respond?

No. Not yet :)

## Load Balancer Configuration

Creating the VMSS also sets up the load balancer, which is a networking component. It listens on the Public IP address and routes incoming traffic to one of the VMSS instances.

Why doesn't the app work? Because the VMSS setup doesn't include any load balancing rules by default - so the LB has no routing table and doesn't know where to send traffic.

Open the LB in the portal:

- select _Backend pools_ and confirm the VMSS instances are all there & running
- select _Health probes_ - this is how the LB checks that resources in the backend pool are ready to receive traffic; no probes so far
- select _Load balancing rules_ - none, which is why the app doesn't work, the LB receives requests but has no rules to forward on to the backend

📋 Add a rule to listen on the frontend PIP and route to the VMSS backend pool, using port 80.

<details>
  <summary>Not sure how?</summary>

Click to add a load balancing rule, and give it any name. Then:

- select the PIP for the frontend
- select the VMSS for the backend pool
- enter `80` for the port and the backend port
- you'll also need to add a health probe - choose the HTTP type

</details><br/>

Load balancers only send traffic to healthy endpoints, which is why you need to include a health probe. 

Browse to your PIP again - now you should see the app. If you refresh do you get responses from different VMs?

## Scaling VMSS

There's lots of caching in the browser stack, try using the command line to make a GET request to the PIP:

```
curl http://<pip>
```

Repeat and you should see different VM names in the HTML response, as the load balancer shares the requests between the three VMs. 

📋 Scale up to five instances using the `vmss scale` command. How quickly do they come on line and start serving responses?

<details>
  <summary>Not sure how?</summary>

Check the help text and you'll see it's a pretty simple command - you just set the desired capacity:

```
az vmss scale -g labs-vmss-win -n vmss-app01 --new-capacity 5
```

</details><br/>

Check in the portal - you'll see new instances listed in VMSS blade, and they will automatically get added to the LB backend pool too. When they are healthy, they'll become valid targets for the LB.

> They'll be creating for a few minutes; Windows VMs are not as fast to commission as Linux

You may also see more than 5 instances in the VMSS - why? Azure overprovisions - it knows there is variation in VM startup time, so it creates more than you need and when the desired count are online, it removes the extra ones which may still be starting up (this is why the instance numbers may not be sequential - the missing numbers are the ones which got removed).

Try repeating your curl command a few times to see the new instances sharing the request load:

```
curl http://<pip>
```

We're using manual scale, you can also set autoscale. The only metric you can scale on is CPU - when the instances are working too hard the VMSS will add more; when there's not enough work to go around, some of the instances will be removed.

📋 Change your VMSS setup in the Portal to use autoscaling, with a minimum of 2 VMs and a maximum of 3. The VMSS should scale up if CPU is greater than 10% and scale down if it's less then 8%, with a short timescale of 2 minutes.

<details>
  <summary>Not sure how?</summary>

- open the vmss in the portal and the _Scaling_ blade
- switch to _Custom autoscale_ 
- select _Scale based on a metric_
- set minimum 2, max 3, default 2
- add a rule to scale out - increase by one instance if avg cpu > 10%
- add a rule to scale in - decrease by one instance if avg cpu < 8%
- use a 2 minute timescales to see changes quickly

</details><br/>

Your VMSS has 5 instances before you switch to autoscale. What happens to the instance count afterwards? Does the app still work when a scaling event is in progress?

After a few minutes you should see two instances deleting, to bring down to the maximum of 3; those deleting instances are removed from the LB. After a few minutes with no activity, another 1 is removed bringing to the minimum of 2.

> Do the most recently added VMs get removed? What would happen if a VM was handling a request when it got deleted?

## Lab 
 
Health probes in the load balancer are a powerful feature for managing lots of failure scenarios. You should test that your application works correctly if instances are unhealthy. With the VMSS you can connect to an instance with Remote Desktop and take the web server offline by stopping the IIS Windows Service. Try that and see if the load balancer works as expected. Can you see the probe status in the portal?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG, which will delete the VMSS  - when the VMSS is deleted it deletes all the VMs:

```
az group delete -y -n labs-vmss-win
```