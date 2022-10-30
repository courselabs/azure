# Lab Solution

We need to set the Storage Account firewall rules, so only traffic from the subnet is allowed. The Web App isn't deployed in the subnet but the VNet integration will take care of that.

## In the Portal 

Open the Storage Account. Under _Networking_ set to _Disabled_ and verify that the app is  now broken.

Then switch to _Enabled from selected virtual networks and IP addresses_ and add the subnet. Verify the app is working again.

> It takes a minute or two for these changes to take effect, but you need to make sure they are having the expected resut

## With the CLI

```
# turn public access off:
az storage account update -g labs-vnet-apps --default-action Deny -n <sa-name>
```

Now the app should be broken.

```
# add a rule to allow your IP address:
az storage account network-rule add -g labs-vnet-apps  --vnet vnet1 --subnet subnet1 --account-name <sa-name>

# check the rules
az storage account network-rule list -g labs-vnet-apps --account-name <sa-name>
```

And the app will work again.