
Easy option - get the IP address of the webapp and add it to the KeyVault firewall:

```
# show details of the app - in here you'll see 'outbound IP addresses':
az webapp show -n assetmanageres3 -g labs-vnet-apps2

# get the outbound IPs:
az webapp show -n assetmanageres3 -g labs-vnet-apps2 --query outboundIpAddresses -o tsv

# add them to the firewall for the KeyVault - use a space-separated list:
az keyvault network-rule add -g labs-vnet-apps2 --name vnetappses #<kv-name> --ip-address <ip1> <ip2>
```

Try the app and it should work. But IP addresses change, so it's better to add vnet integration to the web app.

Remove the IP address rule:

```
az keyvault network-rule remove -g labs-vnet-apps2 --name vnetappses #<kv-name> --ip-address <ip1> <ip2>
```

Try the app again and you'll see it still works; it has cached the config settings so it doesn't need to query KeyVault again. Restart the app and try again and you'll see the application error:

```
az webapp restart -n assetmanageres3 -g labs-vnet-apps2 
```

Add vnet integration and the web app will use the subnet which has keyvault access:

```
az webapp vnet-integration add --vnet vnet1 --subnet subnet1 -g labs-vnet-apps2  -n assetmanageres3 #<dns-unique-app-name>

# check the app:
az webapp show -n assetmanageres3 -g labs-vnet-apps2 
```

Outbound IPs are still public IP addresses but there's a vnet set.

Now try the app and it works again.
