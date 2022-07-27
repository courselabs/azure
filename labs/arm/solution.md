
- labs/arm/lab/azuredeploy.json

Changes allocation method to static and sets the desired IP address together with the IP address version

```
az group create -n labs-arm-lab  --location westeurope
```

Deploy the template:

```
az deployment group create --name vm-simple-linux -g labs-arm-lab  --template-file labs/arm/lab/azuredeploy.json  --parameters @labs/arm/vm-simple-linux/azuredeploy.parameters.json adminPasswordOrKey='ssdfDSDS7777**' dnsLabelPrefix=labs-arm-lab-es
```

Check with a what-if when the deployment has completed:

```
az deployment group create --name vm-simple-linux -g labs-arm-lab  --template-file labs/arm/lab/azuredeploy.json  --parameters @labs/arm/vm-simple-linux/azuredeploy.parameters.json adminPasswordOrKey='ssdfDSDS7777**' dnsLabelPrefix=labs-arm-lab-es --what-if
```
