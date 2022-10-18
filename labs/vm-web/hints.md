# Lab Hints

Azure lets you allocate an IP address and retain it if it's not in use. That's called a _static IP address_ and it's an attribute of the PIP. 

You can update the PIP with the Portal or the command line (see [`az network public-ip update`](https://learn.microsoft.com/en-us/cli/azure/network/public-ip?view=azure-cli-latest#az-network-public-ip-update)), but you can only change it if it's not being used by a VM.

> Need more? Here's the [solution](solution.md).