# Lab Hints

The VMs all share one NSG, so you just need to add the HTTP rule once. When you've tried the three VMs separately - you'll see the VM name in the web page - you can create the Traffic Manager Profile.

Traffic Manager is a pretty simple service. It basically creates a DNS name and lets you link that name to other resources. When you create it you'll need to add the three VMs as endpoints. The Portal makes that pretty straightforward - it will even tell you when there's a problem you need to fix :)

> Need more? Here's the [solution](solution.md).