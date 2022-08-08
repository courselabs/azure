
## Reference

- [App Service healthchecks](https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check?tabs=dotnet)

- [Web app settings & environment variables](https://docs.microsoft.com/en-us/azure/app-service/reference-app-settings?tabs=kudu%2Cdotnet)

## Deploy API with bad config


az group create -n labs-appservice-config -l westeurope --tags courselabs=azure

az appservice plan create -g labs-appservice-config -n app-plan-01 --is-linux --sku B1 --number-of-workers 2

cd src/rng/Numbers.Api

az webapp up -g labs-appservice-config --plan app-plan-01 --os-type Linux --runtime dotnetcore:6.0 -n rng-api-es2 # dns name 

> Test the app:

curl https://rng-api-es2.azurewebsites.net/rng

Check default app config in src/rng/Numbers.Api/appsettings.json

Add a config setting so the API fails after 3 calls:

az webapp config appsettings set --settings FailAfterCallCount='3' -g labs-appservice-config -n rng-api-es2  #<dns-unique-app-name>

> Output includes default settings for web app - not values from config JSON

Repeat until the app fails - how many calls will it take?

curl https://rng-api-es2.azurewebsites.net/rng


Only 3... Should be 2 workers?

az appservice plan show -g labs-appservice-config -n app-plan-01

> `webapp up` includes SKU & capacity params, if left off then defaults will overwrite current plan...

App will stay broken now

Confirm:

curl -v https://rng-api-es2.azurewebsites.net/healthz

## Add healthcheck

Portal - health check tab; enable, set path to `/healthz`, reduce timeout to minimum

Save changes, open metrics view, can view count of HTTP response codes & errors

Try the health endpoint again:

curl -v https://rng-api-es2.azurewebsites.net/healthz

OK again - but only because the healthcheck caused a restart

curl https://rng-api-es2.azurewebsites.net/rng # repeat until fails

Check the portal - it says the app is unhealthy and won't be restarted because there's a single instance.

Scale up:

az appservice plan update -g labs-appservice-config -n app-plan-01 --sku B1 --number-of-workers 2


Repeat the curl /rng until once instance is broken, then check /healthz repeatedly - only one instance will respond


Now break that instance:

curl https://rng-api-es2.azurewebsites.net/rng #repeat

And check with /healthz - the new unhealthy instance still gets all traffic. App service will restart after 1 hour

> Extra details around failing healthchecks on a plan with multiple apps - logic as you would expect

Resetting the single instance:

curl https://rng-api-es2.azurewebsites.net/reset

Now working again, no way to reset other instance through app logic - it's out of the LB and can't be individually accessed

Restart from az:

az webapp restart -g labs-appservice-config -n rng-api-es2

Try with /healthz - two *new* instances will come online

## Auto-heal

Using auto-heal you can trigger restarts of failed instances - the workers for the app get replaced, rather than the hosts in the app plan being restarted

Very awkward UX; in the portal open _Diagnose & solve problems_ and then _Diagnostic Tools_.

Click on _Auto-Heal_, enable it and add a rule:

- if 1 request has a status code of 500
- in a 30-second window
- recycle the process.

Save the settings and break the app with calls to /rng; then try /healthz - you'll see new instances replace the failed ones.

## Lab

general config

Open portal, configuration, _General Settings_

- runtime version, startup command
- FTP deployment, always on - defaults not ideal

Turn FTPS deployment off and always on on using the CLI.
