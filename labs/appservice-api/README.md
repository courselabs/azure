

## Deploy API

az group create -n labs-appservice-api -l westeurope --tags courselabs=azure

az appservice plan create -g labs-appservice-api -n app-plan-01 --is-linux --sku B1 --number-of-workers 2

cd src/rng/Numbers.Api

az webapp list-runtimes

az webapp up -g labs-appservice-api --plan app-plan-01 --os-type Linux --runtime dotnetcore:6.0 -n rng-api-es # dns name 


> Go to portal, app, deployment center, logs - see "Oryx build" for build output

az webapp log deployment show -n rng-api-es -g labs-appservice-api

Browse to <url>/swagger to see API docs

Fetch a random number:

curl -L <url>/rng



## Deploy Website

We also have a web app which consumes the API - we can deploy it as a separate app in the same app service:

cd ../Numbers.Web

az webapp up -g labs-appservice-api --plan app-plan-01 --os-type Linux --runtime dotnetcore:6.0 -n rng-web-es # dns name 

> Browse, it will fail - API service unavailable

Update config - portal, configuration, application settings

Add a new setting:

- key: `RngApi__Url`
- value: `<your-api-url>/rng` (e.g. https://rng-api-es.azurewebsites.net/rng)


Application restart warning, to be sure new config settings get picked up

Try again - now it works (don't need CORS because this is a server-side call)

## Scale the app plan

Try scaling up to 5 instances:

az appservice plan update -g labs-appservice-api -n app-plan-01 --number-of-workers 5

Error. Scale up as much as you can for the SKU:


az appservice plan show -g labs-appservice-api -n app-plan-01 

az appservice plan update -g labs-appservice-api -n app-plan-01 --number-of-workers <capacity-for-sku>

Fetch numbers; check the API logs - different instances responding? OPen website in new private browser - different instance?


## Lab

change url in indes.html & deploy SPA version as web app

_Access to XMLHttpRequest at 'https://rng-api-es.azurewebsites.net/rng?_=1659970485522' from origin 'https://rng-web-spa-es.azurewebsites.net' has been blocked by CORS policy: Response to preflight request doesn't pass access control check: No 'Access-Control-Allow-Origin' header is present on the requested resource._

Edit the API settings to allow access from the new SPA domain
