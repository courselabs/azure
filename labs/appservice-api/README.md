# App Service for Distributed Apps

One App Service Plan can run multiple App Services. That's a good way to share the compute - you can pay for one set of infrastucture and use it to run multiple components of a distributed application. Connecting components together is where things get interesting, because the addresses to use will change for each environment. App Service gives you a nice way to manage that, by setting configuration values in the App Service which get pushed into the application.

In this lab we'll deploy a web front end with a backend REST API as two applications in the same service plan, and configure them to talk to each other.

## Reference

- [App Service configuration settings](https://learn.microsoft.com/en-us/azure/app-service/configure-common?tabs=portal)

- [Configuration sources in .NET 6.0](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0)

- [`az webapp up` command](https://learn.microsoft.com/en-us/cli/azure/webapp?view=azure-cli-latest#az-webapp-up)

- [`az appservice plan update` command](https://learn.microsoft.com/hu-hu/cli/azure/appservice/plan?view=azure-cli-latest#az-appservice-plan-update)

## Deploy API

We'll start with the REST API. It's a random number generator, so when we get it running we should be able to send an HTTP GET request and see a random number in the response.

📋 Create a Resource Group and an App Service Plan. Set the plan to use Linux, with the B1 SKU and two workers.

<details>
  <summary>Not sure how?</summary>

The RG is the usual command:

```
az group create -n labs-appservice-api --tags courselabs=azure -l westeurope 
```

For the App Service Plan, print the help text to see the options:

```
az appservice plan create --help
```

And create with the required settings:

```
az appservice plan create -g labs-appservice-api -n app-plan-01 --is-linux --sku B1 --number-of-workers 2  -l westus
```

</details><br/>

> Not all regions support all the App Service Plan SKUs. The `az` output will show an error _Plan with requested features is not supported in current region_ if you choose a region which doesn't support B1 SKU.

When you have your App Service Plan created, we'll use `az webapp up` to deploy the app. It's a shortcut to creating a web app with local deployment - it creates the App Service and uploads your code all in one command.

```
# switch to the folder with the API source code:
cd src/rng/Numbers.Api

# find the runtime we need - this is a .NET 6.0 app:
az webapp list-runtimes --os-type=linux
```

📋 Use `az webapp up` to deploy the API code to your App Service Plan. Make sure you specify the correct app runtime and OS.

<details>
  <summary>Not sure how?</summary>

The `webapp up` command will create an App Service Plan if you don't specify one. If you do provide an existing plan, you still need to set the OS and runtime, as well as a unique DNS name:

```
az webapp up -g labs-appservice-api --plan app-plan-01 --os-type Linux --runtime dotnetcore:6.0 -l westus -n <api-dns-name>
```

</details><br/>

In the output you'll see the command creates a ZIP file of the source code folder and uploads it.

Browse to the new App Service in the Portal. Open the _Deployment Center_ and check the logs of the current deployment. You'll see the .NET compiler output under the "Oryx build" logs.

You can also print a summary of the deployment logs from the CLI:

```
az webapp log deployment show -g labs-appservice-api -n <api-dns-name>
```

Find the full DNS name and browse to the App Serivce at `https://<api-fqdn>/swagger` and you'll see the API documentation for the random number generator (this uses [Swagger](https://swagger.io) which is a standard tool for documenting REST APIs).

You can navigate the Swagger docs to call the `rng` service, which returns a random number. You can also do that on the command line with curl:

```
curl https://<api-fqdn>/rng
```

> You should see a different random number with each call

## Deploy Website

We have a web front end (also .NET 6.0) which consumes the API - we can deploy it as a separate app in the same app service:

```
cd ../Numbers.Web

az webapp up -g labs-appservice-api --plan app-plan-01 --os-type Linux --runtime dotnetcore:6.0 -l westus -n <web-dns-name> 
```

This will also take a few minutes to deploy. While it's running, check the App Service for the API on the Portal. Can you find the logs showing that you've called the API?

When your web app deployment completes, open the web URL in a browser. You should see the random number app:

![Random Number Generator web UI](/img/rng-web.png)

Click the _Go!_ button to get a random number, and the website will show an error - _RNG service unavailable_. The website also shows the URL it's using to talk to the API which is where the problem is. It's been deployed with developer settings, so we need to update the configuration.

In the Portal open the App Service for the website and open the _Configuration_ settings. You'll see some config settings already there which are used in the deployment, these aren't used by the application. 

Add a new _Application setting_ to override the default URL for the API:

- key: `RngApi__Url`
- value: `https://<api-fqdn>/rng` (this is the URL you used in curl, e.g. mine is https://rng-api-es.azurewebsites.net/rng)

Click _Save_ to update the configuration - you'll see a warning that the app will restart, to make sure it picks up the new configuration settings. 

Browse to the web app again and click the _Go!_ button - now you should get a random number from the API shown in the web page. (If you're a .NET developer you can see the default API URL in the [config file](/src/rng/Numbers.Web/appsettings.json). How does the new setting from the App Service get read by the app?).

## Scale the App Service Plan

We created the App Service Plan with the Basic B1 SKU and 2 instances. We should be able to scale up to a maximum of 3 instances with this pricing tier:

```
# you'll see an error when you run this:
az appservice plan update -g labs-appservice-api -n app-plan-01 --number-of-workers 3
```

That's odd. 

📋 Print out the details of the App Service Plan with an `az ... show` command to confirm the SKU and current worker count.

<details>
  <summary>Not sure how?</summary>

```
az appservice plan show -g labs-appservice-api -n app-plan-01 
```

</details><br/>

You'll see it's on the Free F1 tier, which has a maximum of 1 worker. We definitely created it as a B1 SKU with two workers though; what has happened to change the plan?

Update the plan to return to the B1 SKU and use 3 workers:

```
az appservice plan update -g labs-appservice-api -n app-plan-01 --sku B1 --number-of-workers 3
```

> The SKU update happens first which increases the maximum worker count, so then the plan scales to 3

Open the web app again in your browser and fetch some random numbers. Check the API logs - are different instances responding to the requests? If you open the website in a private browser window with no history, do you see responses from a different API server? What about if you use curl to call the API directly?

## Lab

We have an alternative version of the website which we can deploy as a static web app - it's the HTML file in `labs/appservice-api/spa`. Update the `index.html` file to set the URL for your random number API and deploy the app as a static web app. When you have it running, browse to the app and click the button - nothing will happen.

If you open the developer tools in your browser and try again, you'll see an error like this in the console log:

_Access to XMLHttpRequest at 'https://rng-api-es.azurewebsites.net/rng?=1659970485522' from origin 'https://rng-web-spa-es.azurewebsites.net' has been blocked by CORS policy: Response to preflight request doesn't pass access control check: No 'Access-Control-Allow-Origin' header is present on the requested resource._

This is a security feature - the API won't let any external domains call it directly. Find the setting you need to configure on the API App Service to allow calls from your static web app domain.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG to clean up:

```
az group delete -y -n labs-appservice-api --no-wait
```