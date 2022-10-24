# App Service Configuration and Administration

App Service is a PaaS which gives you features that would take a lot of effort to implement with an IaaS deployment. Your applications will need different configuration settings and App Service lets you set them without manually logging in and editing files. App Service is also able to monitor your application heal, restarting instances which are unhealthy.

In this lab we'll deploy the random number generator REST API with a configuration that causes it to repeatedly fail, and we'll see how App Service can keep the app online.

## Reference

- [App Service healthchecks](https://docs.microsoft.com/en-us/azure/app-service/monitor-instances-health-check?tabs=dotnet)

- [Web app settings & environment variables](https://docs.microsoft.com/en-us/azure/app-service/reference-app-settings?tabs=kudu%2Cdotnet)

- [`az webapp config` commands](https://learn.microsoft.com/en-us/cli/azure/webapp/config?view=azure-cli-latest)

## Deploy an Application which Fails

📋 Start by deploying the .NET 6.0 application in the folder `src/rng/Numbers.Api` to a new App Service Plan in a new Resource Group. Use a Basic SKU with a single worker.

<details>
  <summary>Not sure how?</summary>

Nothing new in the RG:

```
az group create -n labs-appservice-config --tags courselabs=azure
```

The `webapp up` shortcut command will create the App Service Plan, you can specify the SKU -but not the number of workers:

```
az webapp up --help
```

You need to run the command from the source code folder:

```
cd src/rng/Numbers.Api

az webapp up -g labs-appservice-config --plan app-plan-01 --os-type Linux --sku B1 --runtime dotnetcore:6.0 -n <api-dns-name>
```

</details><br/>

Check the details of the App Service Plan:

```
az appservice plan show -g labs-appservice-config -n app-plan-01
```

You should see the current worker count is 1 (which is the default), and the maximum worker count is 3.


Test the app with curl:

```
curl https://<api-fqdn>/rng
```

> The first response may take a minute while the app warms up, but subsequent calls should be fast.

Have a look at the default app configuration which was deployed from the source code:

- [src/rng/Numbers.Api/appsettings.json](/src/rng/Numbers.Api/appsettings.json) - you can set the range for random number generation, and there's also a setting which causes the API to fail after it's been used

📋 Override the default config settings in Azure, using the config key `Rng__FailAfter__CallCount` and the value `3`.

<details>
  <summary>Not sure how?</summary>

You can do this in the Portal with the _Configuration_ page for the App Service.

Or use the CLI:

```
az webapp config appsettings set --settings Rng__FailAfter__CallCount='3' -g labs-appservice-config -n <api-dns-name>
```

</details><br/>

> If you use the CLI, the output prints your new setting together with the app settings created in the deployment

Repeat your curl requests until the app fails - how many calls will it take?

```
curl https://<api-fqdn>/rng
```

After the 3rd call you'll get an error message. This instance of the app is in a failed state now, and it will never heal itself. The API also has a health endpoint (a very useful feature), which you can use to check if it's working correctly:

```
# you should use curl.exe instead if you're on Windows
curl -v https://<api-fqdn>/healthz
```

The `-v` flag shows extra output, including the HTTP status code. Code 500 means _Internal Server Error_ and we can use that to have Azure check the health of the app.

## Add an App Service Healthcheck

We'll do this in the Portal, so it's easier to see what you need to configure. Open the App Service and open the _Health check_ tab (you'll find it under the _Monitoring section_):

- click to enable the health check
- enter `/healthz` as the path - this is the health endpoint of the API
- reduce the load balancing threshold to the minimum value

Save changes - you'll be asked for confirmation as the app will be restarted. Now open the _Metrics_ tab - you can choose to see a graph of the HTTP response codes or the healthcheck status.

This is a new instance of the app, so it's health right now. Try the health endpoint again and this time you'll see a 200 response:

```
# you should use curl.exe instead if you're on Windows
curl -v https://<api-fqdn>/healthz
```

Now fetch some random numbers. The new instance is using the same configuration so after three calls the app will fail again:

```
curl https://<api-fqdn>/rng
```

> Check the portal - you can see a peak in the _Http Server Errors_ metric

In the _Overview_ tab you'll have a red bar at the top saying the app is unhealthy and won't be restarted because there's a single instance. Azure takes a cautious approach and it won't replace a single instace even if it's unhealthy. If it did there would be downtime while the new instance came online to replace it.

## Scale Up the App

Running multiple instances increases the amount of requests your app can handle, and it also improves availability - if one instance has failed then all the incoming traffic is directed to the others.

📋 Scale up the App Service Plan to have two instnaces

<details>
  <summary>Not sure how?</summary>

You can do this in the Portal under the _Scale out_ tab of the App Service Plan, or with the CLI:

```
az appservice plan update -g labs-appservice-config -n app-plan-01 --sku B1 --number-of-workers 2
```

</details><br/>

It will take a few minutes for the new instance to come online. When it does it will be healthy and the previous instance will be unhealthy. Check the health endpoint; as soon as the new instance is ready it will get all the traffic:

```
curl https://<api-fqdn>/healthz
```

Now if you make some calls for random numbers, the only healthy instance will soon become unhealthy too:

```
curl https://<api-fqdn>/rng
```

Both instances are unhealthy now, but the most recently failed one still gets all the traffic. App Service will restart failed instances - but it waits 1 hour to do that...

## Lab

Instead of waiting, you can configure auto-heal which triggers restarts of failed instances. Update the App Service so the API instances will get replaced if there are any 500 errors within a 30-second period.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG to clean up:

```
az group delete -y -n labs-appservice-config
```