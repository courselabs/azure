# Lab Solution

Here are the ARM files exported from my Resource Group:

- [exported/template.json](./lab/exported/template.json) - the resource definitions with default parameter values, e.g. for resource names
- [exported/parameters.json](./lab/exported/parameters.json) - the set of parameters you can vary for a new deployment.

The template can be reused in a new RG, because none of the resources use globally-unique names. Resources can have the same name in different RGs - but the public IP address can't be reused because it's already allocated to your existing VM.

Try to deploy and see if it works:

```
az group create -n labs-vnet2 --tags courselabs=azure -l eastus

az deployment group create --resource-group labs-vnet2 --name deploy1 --template-file ./labs/vnet/lab/exported/template.json
```

You should see an error about IDs not being allowed - fix that and you may see another error about the `requireGuestProvisionSignal` field not being valid in your subscription.

- [updated/template.json](./lab/updated/template.json) - fixes the issues in the exported template

> You'll need to overwrite the SSH details with the ones from your own exported template if you want to deploy this template - it refers to a key which was created on your machine with the `az vm create` command.


```
az deployment group create --resource-group labs-vnet2 --name deploy1 --template-file ./labs/vnet/lab/updated/template.json
```

This should work - and it's repeatable. Run the same command again and the resources won't change. 

The VM in the new RG will have a new public IP address (even though the original one is specified in the ARM template).

Fetch the new IP address and you can connect:

```
az vm show -g labs-vnet2 -n vm01 --show-details --query publicIps -o tsv

ssh <vm01-public-ip>

# inside the VM:
ip address

exit
```

The new VM should have the same private IP address as the other VM, but it's in a different vnet in a different RG.