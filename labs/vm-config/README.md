# Automating VM configuration

All new VMs will need some additional configuration after they're created, and it's not feasible to keep doing that manually. It's time-consuming, error-prone and doesn't scale. Azure gives you multiple options to automate VM configuration as part of the deployment, or after the VM is created. 

In this lab we'll use some simple options for running deployment scripts on Linux and Windows VMs. 

## Reference

- [Azure VM extensions](https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/overview)

- [Azure VM applications](https://docs.microsoft.com/en-us/azure/virtual-machines/vm-applications-how-to?tabs=portal)

- [cloud-init for Azure VMs](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/cloud-init-deep-dive)

- [run commands on VMs](https://docs.microsoft.com/en-us/azure/virtual-machines/run-command-overview)


## Explore VM Configuration in the Portal

Open the Portal and search to create a new Virtual Machine resource. Switch to the _Advanced_ tab and check out the configuration options. There are three configuration mechanisms:

- **extensions** 
- **applications**
- **cloud-init scripts**

These are all mechanisms for configuring the VM.

## Linux with custom script extension

VM extensions are added after the machine is created. One of the most useful is the [custom script extension](https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/custom-script-linux) which can run a shell script.

We'll use a custom script to install Nginx on a web server.

Start by creating a Resource Group and a VM:

```
az group create -n labs-vm-config --tags courselabs=azure -l westeurope

az vm create -l westeurope -g labs-vm-config -n web01 --image UbuntuLTS --size <your-vm-size> --public-ip-address-dns-name <your-dns-name>
```

Custom scripts are specified in JSON. There's an extensive [schema](https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/custom-script-linux#extension-schema) which you can use to provide a file URL, confidential settings for passwords, and more. 

But we'll start simple with a shell command inside a JSON string:

```
{ "commandToExecute": "apt-get -y update && apt-get install -y nginx" }
```

📋 Use a `vm extension` command to run the custom script on your VM using JSON settings.

<details>
  <summary>Not sure how?</summary>

Navigate through the help text and you'll see the `set` command applies the extension:

```
az vm extension --help

az vm extension set --help
```

The syntax is a bit clunky because it's the same command for all extensions, so the spec goes into the `settings` parameter. It's easiest to store the JSON string in a variable, but they are treated differently in Bash and PowerShell:

```
# in PowerShell:
$json='{ ""commandToExecute"": ""apt-get -y update && apt-get install -y nginx"" }'

# or Bash:
json='{ "commandToExecute": "apt-get -y update && apt-get install -y nginx" }'
```

Now you can set the custom script extension to run the shell script on the VM:

```
# add the extension:
az vm extension set -g labs-vm-config --vm-name web01 --name customScript --publisher Microsoft.Azure.Extensions --settings "$json"
```

</details><br/>

You don't get much useful output from the CLI while the extension is being added - but you can browse to the portal and see the progress in the VM _Extensions + applications_ blade.

## Test the web server

When the extension has completed, you can try browsing to the VM - but you won't be able to access it because incoming traffic to port 80 is blocked.

You manage access with the NSG. Use the CLI to find the NG name and print the rules:

```
az network nsg list -g labs-vm-config -o table

az network nsg rule list -g labs-vm-config --include-default -o table --nsg-name <your-nsg-name>
```

> The default rules for a VM include _DenyAllInBound_ which blocks all incoming traffic.

NSG rules have a priority number. You can override the default deny rule by adding another rule with a higher priority.

Rules can be set with IP ranges to allow or block access from specific public addresses. To create a new rule to allow incoming traffic you need to specify:

- the source address prefixes, which can be a special value like `Internet` for all addresses on the public Internet
- the destination port, using `80` for HTTP traffic

📋 Create a new NSG rule to allow incoming access on port 80 to your web VM.

<details>
  <summary>Not sure how?</summary>

The `create` command adds a new rule:

```
az network nsg rule create --help
```

Create a rule with higher priority than the default deny rule to allow access on port 80:

```
az network nsg rule create -g labs-vm-config --nsg-name web01NSG -n http --priority 100 --source-address-prefixes Internet --destination-port-ranges 80 --access Allow
```

</details><br/>

> When the rule has been added you can browse to your VM's DNS name and see the Nginx page.

## Windows with VM extension

Windows VMs also support extensions. The [Windows custom script extension](https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/custom-script-windows) has a slightly different configuration to the Linux version, but the approach is broadly the same.

An easier option is to use `vm run-command`, which can read a local script file and execute it on the VM. We can use the file `labs/vm-win/setup.ps1` to deploy the dev tools on a Windows VM.

📋 Create a new Windows 11 VM and run a custom script extension to deploy the dev tools.

<details>
  <summary>Not sure how?</summary>

First create the VM - be sure to use a VM size you have access to, the latest Windows 11 image and a strong password:

```
az vm create -l westeurope -g labs-vm-config -n dev01 --image <windows-11-image> --size Standard_D4s_v5 --admin-username labs --public-ip-address-dns-name <your-unique-dns-name> --admin-password <your-strong-password>
```

When the VM is created you can run the command:

```
az vm run-command invoke  --command-id RunPowerShellScript -g labs-vm-config --name dev01 --scripts @labs/vm-win/setup.ps1
```

</details><br/>

> When the command completes it prints the output from the script. It's not very friendly but you should see `Chocolatey installed 3/3 packages` so you know the setup is complete.

You can connect to your VM with an RDP client to confirm the tools are installed.

## Lab

You can run full scripts with `run-command` and there are some built-in commands which mean you don't need a script. That's very useful for quick debugging.

We've created two VMs but we didn't specify anything to connect them together across a network. They are in the same Resource Group so you may think they are connected together anyway. Run commands in the Linux and  Windows VMs to print the IP addresses and see if they can reach each other across a private network.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG with this command and that will remove all the resources:

```
az group delete -y --no-wait -n labs-vm-config
```