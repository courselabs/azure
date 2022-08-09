
Portal 

- _Networking_ tab
- choose _Enabled from selected..._ 
- add your client IP address.

CLI:

az storage account update -g labs-storage -n labsstoragees  --public-network-access Disabled

# find your public IP address (or browse to https://www.whatsmyip.org)
curl ifconfig.me

az storage account network-rule list -g labs-storage --account-name labsstoragees

az storage account network-rule add -g labs-storage --account-name labsstoragees --ip-address 213.18.157.115 #<public-ip-address>


## Verify you can still download it:

curl -o download3.txt https://labsstoragees.blob.core.windows.net/newcontainer/document.txt

cat download3.txt

> document text

## Verify VM cannot download

az vm list -o table -g labs-storage --show-details

ssh <vm01-ip-address>

curl -o download4.txt https://labsstoragees.blob.core.windows.net/newcontainer/document.txt

cat download4.txt

> Not authorized