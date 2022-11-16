# Application Insights

Consumption model for metrics, log tracing and dependency visualization

## Create Application Insights

portal new "application insights"

- usual dns name, region
- mode is classic or workspace

Workspace refers to Log Analytics, which is the latest operations tool for storing and analysing data - . We'll create the workspace and then app insights in the cli:

```
az group create -n labs-appinsights  -l eastus --tags courselabs=azure

az monitor log-analytics workspace create -g labs-appinsights -n labsloganalytics 

az monitor app-insights component create --app labs --location eastus --kind web -g labs-appinsights --workspace labsloganalytics
```

Open in Portal

Get the connection string:

```
az monitor app-insights component show --app labs -g labs-appinsights --query connectionString -o tsv
```

## Deploy App with Custom App Insights 

Create some ACI containers running an app which sends monitoring data to app insights.

We'll use a different resource group for the apps:

```
az group create -n labs-appinsights-apps  -l eastus --tags courselabs=azure
```


```
az container create -g labs-appinsights-apps  --image courselabs/fulfilment-processor:appinsights-22.11 --no-wait --name fp1 --secure-environment-variables "ApplicationInsights__ConnectionString=<appinsights-connection-string>"

az container create -g labs-appinsights-apps  --image courselabs/fulfilment-processor:appinsights-22.11 --no-wait --name fp2 --secure-environment-variables "ApplicationInsights__ConnectionString=<appinsights-connection-string>"

az container create -g labs-appinsights-apps  --image courselabs/fulfilment-processor:appinsights-22.11 --no-wait --name fp3 --secure-environment-variables "ApplicationInsights__ConnectionString=<appinsights-connection-string>"

az container create -g labs-appinsights-apps  --image courselabs/fulfilment-processor:appinsights-22.11 --no-wait --name fp4 --secure-environment-variables "ApplicationInsights__ConnectionString=<appinsights-connection-string>"
```

Open the _Live metrics_ view in application insights. How is this app looking?

What can you find out about this app's dependencies from the _Application map_ view?

Can you see the average time to process a batch from the _Performance_ view? Does that reflect the typical processing time or are there outliers skewing the average?

## Add Application Insight to a web app

RNG app built without app insights sdk; but PaaS services can enlist:

```
cd src/rng/Numbers.Web

az webapp up -g labs-appinsights-apps --plan rng-plan-01 --os-type Linux --runtime dotnetcore:6.0 -l westus -n <web-name>
```

Open your Web App in the Portal when it's ready, to _Application Insights_ and click _Turn on APplication Insights_:

- choose _Select existing resource_ and choose the Application Insights instance from this lab
- under _Instrument your applcation_ select _.NET Core_ as the runtime
- click _Apply_


Browse and refresh; try the button - fails


Back in App insights - _Failures_ view; filter by Role=<web-name> and you should see the failed dependency. Click the failure and the _End-to-end transaction details page loads_. Explore these features:

- _Show what happened before and after_
- _Show timeline for this user_

Can you drill down into the error log from these views?

> To fix the app we need to run the API as well and set the API URL in configuration


## Add a REST API to App Insights

```
cd ../Numbers.Api

az webapp up -g labs-appinsights-apps --plan rng-plan-01 --os-type Linux --runtime dotnetcore:6.0 -l westus -n <api-name>
```


Get the URL for the RNG API

In web app configuration, add two appsettings:

- `RngApi__Url` = `https://<your-api/rng`
- `APPINSIGHTS_JAVASCRIPT_ENABLED` = `true` - for additional AppInsights

Save settings. 

Open the API web app and add it to the same app insights.

Try the RNG app now, get a few numbers and see what you get in app insights:

- open _User flows_ and search on the GET /rng event to map out the user workflow
- open _Metrics_ and try some graphs 
    - there are client-side metrics under _BROWSER_ 
    - and under _CUSTOM_ you can see the queue size for the fulfilment processor

In your browser open developer tools and look at the network flow when you refresh the page. What extra traffic is there now that `APPINSIGHTS_JAVASCRIPT_ENABLED` is set?


>

Client-side library makes a `track` call with a payload like this:

```
[{"time":"2022-11-16T15:10:51.770Z","iKey":"f5a9b948-cdf7-4763-8b27-da83fcd7d1c5","name":"Microsoft.ApplicationInsights.f5a9b948cdf747638b27da83fcd7d1c5.Pageview","tags":{"ai.user.id":"QX+GXnwjWg5rTvOHCLqk+F","ai.session.id":"gF1hYn5gDg2IkjRq83Vz0w","ai.device.id":"browser","ai.device.type":"Browser","ai.operation.name":"/","ai.operation.id":"8fae7b0c2b0b4501ae8c09b3d390e0fe","ai.internal.sdkVersion":"javascript:2.8.9","ai.internal.snippet":"4","ai.internal.sdkSrc":"cdn2"},"data":{"baseType":"PageviewData","baseData":{"ver":2,"name":"Courselabs - Numbers.Web","url":"https://clabsazes2211163.azurewebsites.net/","duration":"00:00:00.253","properties":{"refUri":"https://sandbox-8-2.reactblade.portal.azure.net/"},"measurements":{},"id":"8fae7b0c2b0b4501ae8c09b3d390e0fe"}}},{"time":"2022-11-16T15:10:51.771Z","iKey":"f5a9b948-cdf7-4763-8b27-da83fcd7d1c5","name":"Microsoft.ApplicationInsights.f5a9b948cdf747638b27da83fcd7d1c5.PageviewPerformance","tags":{"ai.user.id":"QX+GXnwjWg5rTvOHCLqk+F","ai.session.id":"gF1hYn5gDg2IkjRq83Vz0w","ai.device.id":"browser","ai.device.type":"Browser","ai.operation.name":"/","ai.operation.id":"8fae7b0c2b0b4501ae8c09b3d390e0fe","ai.internal.sdkVersion":"javascript:2.8.9"},"data":{"baseType":"PageviewPerformanceData","baseData":{"ver":2,"name":"Courselabs - Numbers.Web","url":"https://clabsazes2211163.azurewebsites.net/","duration":"00:00:00.253","perfTotal":"00:00:00.253","networkConnect":"00:00:00.000","sentRequest":"00:00:00.189","receivedResponse":"00:00:00.001","domProcessing":"00:00:00.063","properties":{},"measurements":{"duration":253.30000000004657}}}}]
```

Without any changes to code, we get all this from Application Insights, because Azure is hosting the application - it's able to inject the JavaScript for the client, and monitor the traffic in the server.


## Lab

If you create Functions Apps, Azure creates you a separate Application Insights app for each one. In this lab we've created one App Insights instance which we're using for multiple components. Which scenarios do the different approaches support?


## Cleanup 

Delete all the apps in their rg: but leave log analytics there:

```

```