# Lab Solution

## With the Portal

Open your Storage Account:

- select the _Networking_ blade
- choose _Enabled from selected virtual networks and IP addresses_ 
- add your client IP address

## Or with the CLI:

```
# turn public access off:
az storage account update -g labs-storage -n labsstoragees --default-action Deny

# find your public IP address (or browse to https://www.whatsmyip.org)
curl ifconfig.me

# check existing rules
az storage account network-rule list -g labs-storage --account-name labsstoragees

# add a rule to allow your IP address:
az storage account network-rule add -g labs-storage --account-name labsstoragees --ip-address 213.18.157.115 #<public-ip-address>
```

## Verify you can download

From your own machine:

```
curl -o download4.txt https://labsstoragees.blob.core.windows.net/drops/document.txt

cat download4.txt
```

> You should see the document text, not an XML error string

## Verify VM cannot download

Get your VM details and connect:

```
az vm list -o table -g labs-storage --show-details

ssh <vm01-ip-address>
```

Inside the VM terminal, try to download the file:

```
curl -o download5.txt https://labsstoragees.blob.core.windows.net/drops/document.txt

cat download5.txt
```

> You'll see an XML _AuthorizationFailure_ message