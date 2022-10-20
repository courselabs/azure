# Lab Hints

You can start with the existing VM definition in the `vm-simple-linux` folder as a starting point, and edit the network settings. 

ARM templates are trikcy to work with, but the schema is well documented. Here you'll find the options you can use to configure the Netork Interface (NIC) for a VM: [NIC schema](https://docs.microsoft.com/en-us/azure/templates/microsoft.network/networkinterfaces?tabs=json)

When you author a new template it's often an iterative process. The deployment may set default values which are not in the template, and the what-if will want to remove them If you want a repeatable deployment it's better to explicitly set those value in the template.