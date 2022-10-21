# Lab Solution

The sample solution changes the path to the script URI and adds an output line:

- [lab/vm.bicep](labs/iaas-bicep/lab/vm.bicep)

It uses the [environment](https://learn.microsoft.com/en-gb/azure/azure-resource-manager/bicep/bicep-functions-deployment#environment) function to avoid hard-coding the domain name, with string interpolation to build up the full URI:

```
'https://courselabspublic.blob.${environment().suffixes.storage}/iaasbicep/vm-setup.ps1'
```

And it adds an [output](https://learn.microsoft.com/en-gb/azure/azure-resource-manager/bicep/outputs?tabs=azure-powershell#define-output-values) which uses the FQDN from the PIP, again with string interpolation to build the full URL:

```
output url string = 'http://${publicIPAddress.properties.dnsSettings.fqdn}/signup'
```

If you run the deployment command again, you will see a new output with the full URL which you can browse to and check the application.

Repeating the deployment of a VM with a run-command doesn't repeat the command again - if you print the log file you should see the same timestamp from the first time you ran it.