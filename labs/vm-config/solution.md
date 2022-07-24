# Lab Solution

On the Windows VM:

```
az vm run-command invoke  --command-id IPConfig -g labs-vm-config --name dev01
```

And on the Linux VM:

```
az vm run-command invoke  --command-id ifconfig -g labs-vm-config --name web01
```

The output isn't easy to read, but if you look for the internal IP address, they'll be in the range `10.0.0.x`. Both machines have the same subnet, so it looks like they are connected together. 

You can test that with a ping from the Windows VM to the Linux VM:

```
az vm run-command invoke  -g labs-vm-config --name dev01 --command-id RunPowerShellScript --scripts "ping <linux-vm-ip>"
```

Browse to the Portal and you'll see how the VMs are connected.


> Back to the [exercises](README.md).