# Lab Solution

Ensure your VM is deallocated before you change the PIP:

```
az vm deallocate -g labs-vm-web -n vm01
```

When that completes, you can change the _allocation method_ of the PIP, to request a static IP address instead of the dynamic address:

```
az network public-ip update -g labs-vm-web -n vm01PublicIP --allocation-method Static
```

Now check the details of the PIP and you will see the IP address. That will be the address of any VM which uses the PIP and it won't change until you remove the PIP.


> Back to the [exercises](README.md).