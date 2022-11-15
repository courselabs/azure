# Lab Solution

Origin group - same properties, only the name changes:

```
az afd origin-group create -g labs-frontdoor --origin-group-name pi-web --profile-name labs --probe-request-type GET --probe-protocol Http    --probe-interval-in-seconds 30 --probe-path /  --sample-size 4  --successful-samples-required 3 --additional-latency-in-milliseconds 50
```

Origin - there is only one this time:

```
az container show -g labs-frontdoor --name pi --query 'ipAddress.fqdn'

az afd origin create -g labs-frontdoor --profile-name labs --origin-group-name pi-web --origin-name container1 --priority 1 --weight 100 --enabled-state Enabled  --http-port 80 --origin-host-header <pi-fqdn> --host-name <pi-fqdn>
```

Endpoint:

```
az afd endpoint create -g labs-frontdoor --profile-name labs --endpoint-name pi-web --enabled-state Enabled
```

Create the route - this makes the app public:

```
az afd route create -g labs-frontdoor --profile-name labs --endpoint-name pi-web --forwarding-protocol HttpOnly --route-name spi-web-route --origin-group pi-web --supported-protocols Http --https-redirect Disabled --link-to-default-domain Enabled --enable-compression
```

Check in the Portal, you should see all the new configuration is provisioned and you can browse to the Pi endpoint.
