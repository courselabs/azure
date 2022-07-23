# Automating VM configuration

no good connecting to a VM and running scripts - do it as part of setup

## Reference

- [Azure VM extensions](https://docs.microsoft.com/en-us/azure/virtual-machines/extensions/overview)

- [Azure VM applications](https://docs.microsoft.com/en-us/azure/virtual-machines/vm-applications-how-to?tabs=portal)

- [cloud-init for Azure VMs](https://docs.microsoft.com/en-us/azure/virtual-machines/linux/cloud-init-deep-dive)


## Explore VM Configuration in the Portal

Open the Portal and search to create a new Virtual Machine resource. Switch to the _Advanced_ tab and check out the configuration options. There are three configuration mechanisms:

- **extensions** - 
- **applications** -
- **cloud-init scripts** -


## Linux with VM extension

## Linux with cloud-init

## Windows with VM extension

## Lab


> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG with this command and that will remove all the resources:

```
az group delete -y -n labs-vm-config
```