
## Create a static webapp

az group create -n labs-appservice-static  -l westeurope --tags courselabs=azure

cd labs/appservice-static/html 

az webapp up -g labs-appservice-static --html --sku F1 -n <unique-dns-name> 

> creates app service plan - random name; ZIP deploys from current dir

browse to output - it will take a while to come online

OPen the web app in the portal and check the console - what OS and web server is it hosted on?

Use curl to check how the content is delivered:

curl -IL <url>


> Windows, ASP.NET - default for static web sites; SSL provided with Azure domain name

etag & last modified set automatically & help with caching

Check local FS - .azure folder contains config file for future deployments

## Using Node for Static content


cd ../node 

az webapp up -g labs-appservice-static --os-type Linux --runtime node:16-lts --sku B1 -n <unique-dns-name> 

> new app service plan; requested Linux, current is Windows


Compare scaling options for app service plans

Compare app services - console is SSH for linux

Browse to site & check with vurl:

curl -IL <url>

## Authentication & Authorization

Portal for Node app
 
 Click _Add identity provider_
 - Select Microsoft as the IdP
 - Select _Any Azure AD directory & personal Microsoft accounts_

Open a new private browser window and browse to your Node js URL

> It may take a few minutes for auth to be configured; you'll see a login page when it is

No changes to the app; log in with Azure account - you'll be asked to confirm the app can read account details

browse to /user on the site - you'll see Azure listed as the IdP and your Azure Account email address

## Lab

check the logs for both apps - browse to some non-existent URLs and see how the 404s are reported

(Kudu - IIS web logs for static; node app - docker logs in Log Stream)
