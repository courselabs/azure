# Lab Solution

- create a _Traffic Manager Profile_ and give it a unique DNS name

- open the Traffic Manager and browse to _Endpoints_

- add one of the Public IP Addresses for your VM as an endpoint

> You'll get an error telling you the PIP does not have a DNS name

DNS names are required because Traffic Manager just uses round-robin DNS for load-balancing across PIPs.

- add dns names to all the VM PIPs

- add each PIP as an endpoint in Traffic Manager

Try browsing to your Traffic Manager URL. You'll see a response from one VM but if you refresh you may get the same VM responding (there's lots of caching in the HTTP stack).

If you have a Mac or Linux machine, you'll have the `dig` tool available, which prints the DNS response for an address. Check your Traffic Manager address and you'll see how the load balancing is done:

```
# e.g. for my profile:
dig labsvmimagees.trafficmanager.net
```

I see a response like this:

```
;; ANSWER SECTION:
labsvmimagees.trafficmanager.net. 60 IN CNAME   labspipn0.westeurope.cloudapp.azure.com.
labspipn0.westeurope.cloudapp.azure.com. 10 IN A 52.148.241.50
```

And if I repeat it, I see different responses to provide load balancing across the addresses.
