# Application Gateway with Web Application Firewall

Application Gateway is a [layer 7 load balancer](https://www.nginx.com/resources/glossary/layer-7-load-balancing/) - it routes traffic based on incoming HTTP requests, using the domain name and URL path to match to the right backend service. Backends are monitored to make sure they are healthy, and traffic is shared between healthy instances.

Web Application Firewall (WAF) is an optional feature of App Gateway, and it's a powerful security tool. WAF can inspect the header and body of HTTP calls looking for mailicious payloads. Attacks can be prevented at the WAF layer so they never reach the backend service.

In this lab we'll deploy App Gateway with WAF and use it as the front-end for some web applications running in Azure Container Instances.

## Reference

- [App Gateway overview](https://learn.microsoft.com/en-us/azure/application-gateway/overview)

- [Web Application Firewall overview](https://learn.microsoft.com/en-us/azure/web-application-firewall/overview)

- [Fronting API Management with Application Gateway](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-integrate-internal-vnet-appgateway)


## Create Application Gateway

We explored App Gateway in the [AKS Ingress lab](/labs/aks-ingress/README.md), so we'll get straight to creating one with the CLI.

Start with the RG and networking pre-reqs:

```
# rg:
az group create -n labs-appgw --tags courselabs=azure -l eastus

# pip:
az network public-ip create -g labs-appgw -n appgw-pip --sku Standard -l eastus --dns-name <unique-dns-name>

# vnet:
az network vnet create -g labs-appgw -n vnet --address-prefix 10.4.0.0/16 -l eastus

# subnet:
az network vnet subnet create -g labs-appgw --vnet-name vnet -n appgw --address-prefixes 10.4.10.0/24
```

> Application Gateway needs to be deployed inside a vnet

Now create the Application Gateway (AppGW) - we're going to use the Web Application Firewall (WAF) feature, so we start with a WAF policy that will implement OWASP rules (from the organisation behind the [OWASP Top 10](https://owasp.org/www-project-top-ten/)):

```
# create WAF policy using latest 3.2 ruleset
az network application-gateway waf-policy create -n appg-waf  -g labs-appgw  --type OWASP --version 3.2 -l eastus

# enable the policy and set to Prevention mode
az network application-gateway waf-policy policy-setting update --mode Prevention --policy-name appg-waf -g labs-appgw --request-body-check  true --state Enabled 
```

> WAF can operate in _Detection_ or _Prevention_ modes.

Prevention mode blocks any incoming calls which look suspicious according to the WAF rules. In some cases there may be false positives, so the WAF breaks your application and you need to relax some of the rules. Typically you start in Prevention mode and monitor any violations. 

Now create the App Gateway with the WAF SKU:

```
az network application-gateway create -g labs-appgw -n appgw  --public-ip-address appgw-pip --vnet-name vnet --subnet appgw --capacity 1 --sku WAF_v2 --priority "1" --waf-policy appg-waf -l eastus
```

This will take a while to spin up. Check progress in the Portal, but while it's creating we can deploy the backend services which the App Gateway will front.

## Create Backend ACI Containers

We'll use Azure Container Instances as the fastest way to deploy some web apps (we covered this in the [ACI lab](/labs/aci/README.md)).

ACI doesn't scale horizontally, so we'll run two separate containers with the simple web app:

```
az container create -g labs-appgw --name simple-web-1 --image courselabs/simple-web:6.0 --ports 80 --ip-address Public --no-wait

az container create -g labs-appgw --name simple-web-2 --image courselabs/simple-web:6.0 --ports 80 --ip-address Public --no-wait
```

And another one with the Pi web app:

```
az container create -g labs-appgw --name pi-0 --image  kiamol/ch05-pi --ports 80 --ip-address Public --command-line "dotnet Pi.Web.dll -m web" --no-wait
```

When the containers are running, test each of the web apps and make a note of the IP addresses.

## Configure App Routing

Open AppGW in the Portal. Browse to the public URL from the PIP - you'll see a `502` error, which is a routing issue. It means there is no path to a backend which can service the request.

Set up the configuration for the web applications - this needs a few settings with the correct values. We'll use fake domain names.

_For the simple web app create:_

- a listener with domain name `simple.appgw.azure.courselabs.co`
- backend pool with the two simple-web ACI IP addresses
- rule linking the listener and pool

_For the Pi app create:_

- a listener with domain name `pi.appgw.azure.courselabs.co`
- backend pool with the Pi ACI IP address
- rule linking the listener and pool

Add the fake domains to your hosts file **pointing to the IP address of your AppGW**:

```
# using Powershell on Windows - your terminal needs to be running as Admin:
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force
./scripts/add-to-hosts.ps1 pi.appgw.azure.courselabs.co <appgw-ip>
./scripts/add-to-hosts.ps1 simple.appgw.azure.courselabs.co <appgw-ip>

# on macOS or Linux - you'll be asked for your sudo password:
sudo chmod +x ./scripts/add-to-hosts.sh
./scripts/add-to-hosts.sh pi.appgw.azure.courselabs.co <appgw-ip>
./scripts/add-to-hosts.sh simple.appgw.azure.courselabs.co <appgw-ip>
```

> Or edit `/etc/hosts` on *nix or `C:\windows\system32\drivers\etc\hosts` on Windows

Try both web addresses - you should see the apps, and the simple-web app should load-balance multiple requests between the ACI containers:

- http://simple.appgw.azure.courselabs.co
- http://pi.appgw.azure.courselabs.co

## Test the Web Application Firewall

We can test out some of the WAF rules, making sure any malicious requests are blocked. We'll compare the response from the app directly to the response from the AppGW WAF.

This is an attempted [SQL injection](https://owasp.org/www-community/attacks/SQL_Injection) attack:

```
# try with the IP address for a simple-web container:
curl "http://<container-ip>/?id=1;select+1,2,3+from+users+where+id=1--"
```

You'll see standard 200 response with HTML output, which tells a hacker that SQL injection isn't being checked for, and that gives them an avenue to explore.

Try the same app with the same attack through the AppGW WAF:

```
curl "http://simple.appgw.azure.courselabs.co/?id=1;select+1,2,3+from+users+where+id=1--"
```

This time you'll get a 403 forbidden response; the WAF blocks the request from getting to the backend.

If you want a more thorough test, try the [GoTestWAF](https://github.com/wallarm/gotestwaf) tool which you can run in a container. It executes hundreds of attempted attacks against a domain - **so make sure you only use it with domains you own**.

Start Docker Desktop and run this command, using your AppGW IP address:

```
docker run --add-host test.url:<app-gw-ip> sixeyed/gotestwaf:2211 --noEmailReport --url http://test.url --skipWAFIdentification --skipWAFBlockCheck  --testSet owasp
```

> It takes a while, but you should see over 500 test cases all successfully blocked by WAF.

You can dig into the attacks the tool tries from the GitHub repo.

## Lab

AppGW can route based on URL paths, so routes in the same domain get sent to different backends. What part of the routing configuration would you use for that?

> Stuck? Try my [suggestions](suggestions.md).
___

## Cleanup

You can delete the RG from this lab:

```
az group delete -y --no-wait -n labs-appgw
```