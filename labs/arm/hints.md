

NIC schema: https://docs.microsoft.com/en-us/azure/templates/microsoft.network/networkinterfaces?tabs=json

- you may need to iterate because the deployment may set default values which are not in the template, and the what-if will want to remove them; in that case it's better to explicitly set the value in the template