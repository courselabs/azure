# Lab Solution

We need to set the Storage Account firewall rules, so only traffic from the AKS subnet is allowed.

## In the Portal 

Open the Storage Account. Under _Networking_ set to _Disabled_ and verify that the app is  now broken.

Then switch to _Enabled from selected virtual networks and IP addresses_ and add the subnet. Verify the app is working again.

> It takes a minute or two for these changes to take effect, but you need to make sure they are having the expected resut

## With the CLI

```
# turn public access off:
az storage account update -g labs-aks-apps --default-action Deny -n <sa-name>
```

Now the app should be broken.

```
# add a rule to allow the AKS subnet:
az storage account network-rule add -g labs-aks-apps  --vnet appnet --subnet aks --account-name <sa-name>
```

And the app will work again.