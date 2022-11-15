# Azure Front Door

Front Door acts like Application Gateway but it also has a global CDN. It can be integrated with the same WAF functionality and provides DDOS protection.

It's the evolution of various load balancing and CDN services and is the preferred option now for the front end to your HTTP services (which can include Web Apps and API Management domains).

In this lab we'll create and configure Front Door with WAF.

## Reference

- [Front Door overview](https://learn.microsoft.com/en-us/azure/frontdoor/front-door-overview)

- [Choosing between Azure Load Balancers](https://learn.microsoft.com/en-us/azure/architecture/guide/technology-choices/load-balancing-overview?toc=%2Fazure%2Ffrontdoor%2Fstandard-premium%2Ftoc.json)

- [`az afd` commands](https://learn.microsoft.com/en-us/cli/azure/afd?view=azure-cli-latest)

## Explore & create Front Door

Create a new resource in the Portal and search for "front door" - select _Front Door and CDN profiles_ click _Create_ and choose _Quick Create_:

- the tier choices prioritize performance **or** security
- select the _Premium_ tier and you need to choose a WAF policy
- Front Door supports lots of origins - including ACI, APIM and AppGW
- you can get caching on static resources just by ticking a box

We'll create Front Door using the CLI.

Start with a resource group and a Front Door profile:

```
az group create -n labs-frontdoor --tags courselabs=azure -l eastus

az afd profile create --profile-name labs -g labs-frontdoor --sku Premium_AzureFrontDoor
```

The Front Door needs some backends to front - we'll use ACI containers, running the same app in two different regions:

```
az container create -g labs-frontdoor --name simple-web-1 --image courselabs/simple-web:6.0 --ports 80 --no-wait -l eastus --dns-name-label <app1-dns-name>

az container create -g labs-frontdoor --name simple-web-2 --image courselabs/simple-web:6.0 --ports 80 --no-wait -l westus --dns-name-label <app2-dns-name>
```

While the containers are starting, open the Front Door Profile in the Portal:

- _Front Door manager_ is where you create endpoints, which are subdomains (or custom domains) that are the entrypoint to your app
- _Origin groups_ are the backends - each endpoint refers to an origin group, and each origin group can have multiple origins, which are the actual application hosts
- _Routes_ link a frontend endpoint to a backend origin group
- _Rule sets_, _Security policies_ and _Optimizations_ let you customize the processing behaviour for individual routes

## Configure the ACI backends as origins

Each application host (which could be an ACI container, Web App, VM etc) is created in Front Door as an _origin_. Origins belong to an _origin group_ - the group defines settings which are shared across all instances.

Create the origin group for the simple web app:

```
az afd origin-group create -g labs-frontdoor --origin-group-name simple-web --profile-name labs --probe-request-type GET --probe-protocol Http    --probe-interval-in-seconds 30 --probe-path /  --sample-size 4    --successful-samples-required 3 --additional-latency-in-milliseconds 50
```

> These are all required settings

At origin group level you define the health parameters - which URL path to test on, how frequently to test, how many successful responses determine that the origin is healthy, and how much additional latency is acceptable.

Now add the first ACI container as an origin to the group:

```
# print the FQDN of container 1:
az container show -g labs-frontdoor --name simple-web-1 --query 'ipAddress.fqdn'

# add container 1 as an origin using the FQDN:
az afd origin create -g labs-frontdoor --profile-name labs --origin-group-name simple-web --origin-name container1 --priority 1 --weight 300 --enabled-state Enabled  --http-port 80 --origin-host-header <container-1-fqdn> --host-name <container-1-fqdn>
```

And the second:

```
# print the FQDN of container 2:
az container show -g labs-frontdoor --name simple-web-2 --query 'ipAddress.fqdn'

# add container 2 as an origin using the FQDN:
az afd origin create -g labs-frontdoor --profile-name labs --origin-group-name simple-web --origin-name container2 --priority 1 --weight 1000 --enabled-state Enabled  --http-port 80 --origin-host-header <container-2-fqdn> --host-name <container-2-fqdn>
```

> We've added both containers with priority 1, but the first container has a weighting of 300 and the second has a weighting of 1000

The priorities and weightings are used in the load-balancing decision. If both origins are healthy, then the weighting is used - we should see about 3x more traffic going to container 1 than container 2.

Open the Front Door Profile in the Portal again. You'll see the new origin group listed with both the ACI containers as origins. This is just the backend part of the setup - you can see in the origin group table that `simple-web` isn't associated to any routes yet.

## Configure the frontend

Front Door entrypoints are the _endpoints_ - these are the public domain names for your app.

Create an endpoint for the simple web app:

```
az afd endpoint create -g labs-frontdoor --profile-name labs --endpoint-name simple-web --enabled-state Enabled
```

Everything is ready now and the final step is to link the frontend endpoint with the backend origin group by creating a _route_:

```
az afd route create -g labs-frontdoor --profile-name labs --endpoint-name simple-web --forwarding-protocol HttpOnly --route-name simple-web-route --origin-group simple-web --supported-protocols Http --https-redirect Disabled --link-to-default-domain Enabled --enable-compression true
```

> The route configuration is where some of the extra features get applied

ACI doesn't give us an HTTPS endpoint, so we're restricting traffic here to HTTP. We are enlisting compression, so if client browsers send a request which supports compression (which they all do), then Front Door will send a compressed response.

Browse to your Front Door Profile in the Portal again. From the _Overview_ page you should see green ticks with _Provision succeeded_ for:

- your endpoint simple-web
- the route
- and the origin group

Browse to the endpoint URL and refresh a few times - is the load-balancing working in favour of one container?

## Apply WAF security rules

Front Door in the Premium SKU can run the same WAF functionality as App Gateway. You create different WAF security policies for each endpoint, which lets you tailor the ruleset for each frontend.

Start by creating a WAF policy which is enabled and runs in Prevention mode:

```
az network front-door waf-policy create -g labs-frontdoor --name simplewebwaf --sku Premium_AzureFrontDoor --disabled false --mode Prevention
```

> Note that the WAF policy isn't attached to a Front Door Profile - it's a separate resource in the RG

The policy starts off without any rules - you need to select the ruleset(s) that gives you the protection you want. You can list all the available rulesets:

```
az network front-door waf-policy managed-rule-definition list -o table
```

The two most useful are the [Microsoft Defaults](https://learn.microsoft.com/en-us/azure/web-application-firewall/afds/waf-front-door-drs?tabs=drs20#drs21) which cover the OWASP threats, and the [Bot Manager](https://learn.microsoft.com/en-us/azure/web-application-firewall/ag/bot-protection-overview) which blocks access to bots.


Add both those rulesets to the WAF policy:

```
# we'll use an older version of the default rules
# these don't require a Firewall resource
az network front-door waf-policy managed-rules add  -g labs-frontdoor --policy-name simplewebwaf --type Microsoft_DefaultRuleSet --version 1.1

# and the current bot rules
az network front-door waf-policy managed-rules add  -g labs-frontdoor --policy-name simplewebwaf --type Microsoft_BotManagerRuleSet --version 1.0
```

Check the Resource Group in the Portal and you'll see the WAF policy listed. Open it and select the _Managed rules_ menu to see all the rules which will get applied.

Now we can apply the WAF by creating a security policy in the Front Door Profile. The command for this is generic so it needs to use the full resource IDs:

```
# print the endpoint resource ID:
az afd endpoint show -g labs-frontdoor --profile-name labs --endpoint-name simple-web --query id

# print the WAF policy ID:
az network front-door waf-policy show  -g labs-frontdoor -n simplewebwaf --query id

# create the Front Door security policy:
az afd security-policy create -g labs-frontdoor --profile-name labs --security-policy-name simplewebsec --domains <endpoint-id> --waf-policy <policy-id>
```

Check the Front Door in the Portal and you should see _Provision succeeded_ for the Security Policy and the WAF.

Now attacks like SQL injection will be blocked:

```
# on Windows use curl.exe
curl -v "http://<endpoint-url>/?id=1;select+1,2,3+from+users+where+id=1--"
```

You'll get a 403 response with a _request is blocked_ message.

## Lab

Setting up Front Door needs a few steps, but it's much more intuitive than App Gateway. Your turn to add a new application to the profile. Run a new ACI container with the Pi application:

```
az container create -g labs-frontdoor --name pi --image kiamol/ch05-pi --ports 80 --ip-address Public --command-line "dotnet Pi.Web.dll -m web" --no-wait --dns-name-label <pi-dns-name>
```

And follow similar steps to publish the app through your Front Door profile with its own endpoint URL.


> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y --no-wait -n labs-frontdoor
```
