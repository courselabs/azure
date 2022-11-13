# Application Gateway with Web Application Firewall

- layer 7 load balancer with optional WAF

## Reference

- [App Gateway overview](https://learn.microsoft.com/en-us/azure/application-gateway/overview)

- [Fronting API Management with Application Gateway](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-integrate-internal-vnet-appgateway)


## Create Application Gateway

We explored App Gateway in the [AKS Ingress lab](/labs/aks-ingress/README.md), so we'll get straight to creating one with the CLI.

First the RG and networking pre-reqs:

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

Now create the application gateway - we're going to use the Web Application Firewall (WAF) feature, so we start with a WAF policy that will implement OWASP rules (from the organisation behind the [OWASP Top 10](https://owasp.org/www-project-top-ten/)):

```
az network application-gateway waf-policy create -n appg-waf  -g labs-appgw  --type OWASP --version 3.2 -l eastus

az network application-gateway waf-policy policy-setting update --mode Prevention --policy-name appg-waf -g labs-appgw --request-body-check  true --state Enabled 
```

And now the App Gateway with the WAF SKU:

```
az network application-gateway create -g labs-appgw -n appgw  --public-ip-address appgw-pip --vnet-name vnet --subnet appgw --capacity 1 --sku WAF_v2 --priority "1" --waf-policy appg-waf -l eastus
```

This will take a while to create. Check progress in the Portal, but while it's creating we can create the backend services which the App Gateway will front.

## Create Backend ACI Containers

Two containers with the simple web app:

```
az container create -g labs-appgw --name simple-web-1 --image courselabs/simple-web:6.0 --ports 80 --ip-address Public --no-wait

az container create -g labs-appgw --name simple-web-2 --image courselabs/simple-web:6.0 --ports 80 --ip-address Public --no-wait
```

One with Pi:

```
az container create -g labs-appgw --name pi-0 --image  kiamol/ch05-pi --ports 80 --ip-address Public --command-line "dotnet Pi.Web.dll -m web" --no-wait
```

When the containers are running, test them individually and copy the IP addresses.


## Configure App Routing

Open AppGW in the Portal. Browse to the PIP URL - 502 error is a routing issue, it means there is no path to a backend which can service the request.

Set up the configuration for the web app - this needs new settings with the correct values. We'll use fake domain names:

- listener with domain name `simple.appgw.azure.courselabs.co`
- backend pool with the two simple-web ACI IP addresses
- rule linking the listener and pool

- listener with domain name `pi.appgw.azure.courselabs.co`
- backend pool with the two pi ACI IP addresses
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

Try both web addresses - you should see the apps, and the simple-web app should be load-balanced:

- http://simple.appgw.azure.courselabs.co
- http://pi.appgw.azure.courselabs.co

## Test WAF

Test out some of the WAF rules, comparing the response from the apps directly to the AppGW routing

This is an attempted SQL injection attack:

```
# try with the IP address for a simple-web container:
curl "http://<container-ip>/?id=1;select+1,2,3+from+users+where+id=1--"
```

You'll see standard 200 response with HTML output, which tells a hacker that SQL injection isn't being checked.

Try again with the ApPGW WAF:

```
curl "http://simple.appgw.azure.courselabs.co/?id=1;select+1,2,3+from+users+where+id=1--"
```

This time you'll get a 403 forbidden response, the WAF blocks the request getting to the backend.

If you want a more thorough test, try the [GoTestWAF](https://github.com/wallarm/gotestwaf) tool which you can run in a container.

Start Docker Desktop and run this command, using your AppGQ IP address:

```
docker run --add-host test.url:<app-gw-ip> sixeyed/gotestwaf:2211 --noEmailReport --url http://test.url --skipWAFIdentification --skipWAFBlockCheck  --testSet owasp
```

> It takes a while, but you should see over 500 test cases all successfully blocked by WAF.


## Lab

You can use AppGW to route on URL paths, so routes in the same domain get sent to different backends. What part of the routing configuration would you use for that?