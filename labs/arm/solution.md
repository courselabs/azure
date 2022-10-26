# Lab Solution

Here's my sample solution: 
- [lab/azuredeploy.json](/labs/arm/lab/azuredeploy.json)

It changes the IP address allocation method to static and sets a specific IP address together with the IP address version.

Create a new RG to use for the dpeloyment:

```
az group create -n labs-arm-lab --tags courselabs=azure  --location westeurope
```

Deploy the template:

```
az deployment group create --name lab -g labs-arm-lab  --template-file labs/arm/lab/azuredeploy.json  --parameters @labs/arm/vm-simple-linux/azuredeploy.parameters.json adminPasswordOrKey='<strong-password>' dnsLabelPrefix=<unique-dns-name>
```

Check with a what-if when the deployment has completed:

```
az deployment group create --name vm-simple-linux -g labs-arm-lab  --template-file labs/arm/lab/azuredeploy.json  --parameters @labs/arm/vm-simple-linux/azuredeploy.parameters.json adminPasswordOrKey='<strong-password>' dnsLabelPrefix=<unique-dns-name> --what-if
```
