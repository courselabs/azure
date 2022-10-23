# Virtual Machine Scale Sets - Linux

Using a custom image for a VM Scale Set means the instances can start work as soon as they come online - there's no additional setup for them to do. But it makes upgrades more complex, because you need to build a new image every time you have a new release of your app. If your app can be deployed quickly to a base VM image then you can script that deployment instead.

In this lab we'll use the _cloud-init_ system with a Linux VMSS to deploy the app to instances when they start. We'll also see how to automate the load balancer rules for a new VMSS.

## Reference

- [Azure cloud-init support for Linux VMs](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/using-cloud-init)

- [cloud-init config examples](https://cloudinit.readthedocs.io/en/latest/topics/examples.html#)

- [`az network lb rule` commands](https://learn.microsoft.com/en-us/cli/azure/network/lb/rule?view=azure-cli-latest)

## Create a VM with cloud-init

VM Scale Sets are great for running applications at scale, but when you're first setting up your application it's usually easier to start with a single VM and get your configuration worked out.

Start with a new RG for this lab:

```
az group create -n labs-vmss-linux --tags courselabs=azure -l westeurope
```

cloud-init is a powerful cross-platform system for configuring new machines. You can do all the usual steps for deploying pre-requisites, installing applications and writing config files:

- [cloud-init.txt](labs/vmss-linux/setup/cloud-init.txt) - this is a simple init script which installs the Nginx web server

You can include a cloud-init script as a piece of [custom data](https://learn.microsoft.com/en-us/azure/virtual-machines/custom-data) when you create a VM in Azure

📋 Create a new VM from the `UbuntuLTS` image, passing the file `labs/vmss-linux/setup/cloud-init.txt` as the custom data script.

<details>
  <summary>Not sure how?</summary>

You can reference local files in `az` commands with `@<file-path>` syntax:

```
# remember to use a size which is available to you:
az vm create -l westeurope -g labs-vmss-linux -n web01 --image UbuntuLTS --size Standard_A1_v2 --custom-data @labs/vmss-linux/setup/cloud-init.txt --public-ip-address-dns-name <your-dns-name>
```

</details><br/>

When your VM is created, run a command to print the output of the cloud-init script - the log file is written to a standard path:

```
az vm run-command invoke  -g labs-vmss-linux -n web01 --command-id RunShellScript --scripts "cat /var/log/cloud-init-output.log"
```

> You'll see the install log for Nginx

And you can test the web server is listening with another run command:

```
az vm run-command invoke  -g labs-vmss-linux -n web01 --command-id RunShellScript --scripts "curl localhost"
```

## Use cloud-init for Linux VMSS

Now we've seen how cloud-init works, here's a more interesting setup script which we'll use for a VM Scale Set:

- [cloud-init-custom.txt](labs/vmss-linux/setup/cloud-init-custom.txt) - installs Nginx and writes a custom HTML page; cloud-init lets you inject variables into files, this example puts the VM host name into the web page

It's the same custom data approach to use cloud-init files with a VMSS.

📋 Create a VM Scale Set from the Ubuntu image with 3 instances, passing the `cloud-init.txt` file as the custom data script.

<details>
  <summary>Not sure how?</summary>

The command is pretty much the same for a VMSS as for a VM - just adding the number of instances:

```
az vmss create -n vmss-web01 -g labs-vmss-linux --vm-sku Standard_D2s_v5 --instance-count 3 --image UbuntuLTS --custom-data @labs/vmss-linux/setup/cloud-init-custom.txt --public-ip-address-dns-name <unique-dns-name>
```

</details><br/>

We saw in the [VMSS Windows lab](labs/vmss-win/README.md) that the new VM Scale Set is created with a PIP and a load balancer, but the load balancer rules aren't configured so the traffic doesn't go anywhere. 

Print the list of rules to confirm there's nothing set up:

```
az network lb list -g labs-vmss-linux -o table

az network lb rule list -g labs-vmss-linux -o table --lb-name <lb-name>
```

📋 Create a load balancer rule forwarding port 80 - you'll also need a health probe for the same port.

<details>
  <summary>Not sure how?</summary>

Create the health probe first:

```
az network lb probe create -g labs-vmss-linux -n 'http' --protocol tcp --port 80  --lb-name <lb-name> 
```

So you can reference it for the new rule:

```
az network lb rule create -g labs-vmss-linux --probe-name 'http' -n 'http' --protocol Tcp --frontend-port 80 --backend-port 80 --lb-name <lb-name> 
```          

</details><br/>

> Browse to your PIP IP address or DNS name to check the output - it should be the custom HTML page, showing the machine's local name

You may not see load balancing in action with the browser cache, so you can check that with the command line:

```
curl http://<pip-address>
```

You should see responses from all 3 instances when you repeat the command.

## Update VMSS 

VM Scale Sets have functionality to manage updates to your application. The VMSS [model](https://learn.microsoft.com/en-us/azure/virtual-machine-scale-sets/virtual-machine-scale-sets-upgrade-scale-set) stores the desired state of the instance, and each instance can be upgraded when the model changes.

Print the VMSS instance details, and you can see if they are up to date in the `latestModelApplied` field:

```
az vmss list-instances -g labs-vmss-linux -n vmss-web01
```

You can update the VMSS with a change to the desired VM state. That changes the model and means the existing VMs will be out of date. We can see what happens if we change the custom data to use a different cloud-init script:

- [cloud-init-updated.txt](labs/vmss-linux/setup/cloud-init-updated.txt) - changes the HTML file which Nginx returns

Updating the custom data isn't as easy as it should be, because we need to set a JSON field. The CLI doesn't let you use a file in this case, you need to load the file into a Base-64 string:

```
# in PowerShell:
$customData=$(cat labs/vmss-linux/setup/cloud-init-updated.txt | base64)

# OR Bash:
customData=$(cat labs/vmss-linux/setup/cloud-init-updated.txt | base64)
```

Now pass the Base-64 string to the update command, setting it as the VM's custom data field:

```
az vmss update -g labs-vmss-linux -n vmss-web01 --set virtualMachineProfile.osProfile.customData=$customData
```

> Print the instance list after the command completes, or check the instances in the portal 

The instances show that they don't have the latest model applied. **Changes to the model are not automatically applied to existing VMs**. If you scale up you'll see new VMs have the latest model, but old ones need to be explicitly updated.

📋 Update all the instances using an `az vmss` command. 

<details>
  <summary>Not sure how?</summary>

List all the subcommands:

```
az vmss --help
```

This is the command you want - you can update specific instances, or all of them:

```
az vmss update-instances  -g labs-vmss-linux -n vmss-web01 --instance-ids '*' 
```

</details><br/>

Check the instances again and you should see they're all using the latest model. But if you browse to the web page you'll see the HTML page has not changed. The custom data file contents have been updated, but that file is only processed at provisining time - so these VMs won't run the setup again.

Scale up the VMSS - when the new instances come online, they will use the new model and provision the new content:

```
az vmss scale -g labs-vmss-linux -n vmss-web01 --new-capacity 5
```

Make a few curl requests and you'll see different responses from the old and new machines.

## Lab

Our VMSS is in a bad state - all the VMs are on the latest model and they're all healthy, so they're all valid targets for the load balancer. But they're running different versions of our application (the simple HTML page). To fix this you'll need to force the old VMs to be recreated from the current model, which you can do with the Portal or the CLI.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y -n labs-vmss-linux
```