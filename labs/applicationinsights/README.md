# Application Insights

Application Insights is a monitoring tool which combines ingestion of log and metric data, with a UI to search and explore the data. You can add Application Insights support to any app by using the client library, and PaaS platforms (including Web Apps and Function Apps) support automatic instrumentation, so they can send key data to App Insights without any code changes.

Applications send data to App Insights as the central collector, and each Application Insights _app_ is linked to a [Log Analytics](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/log-analytics-overview) service which stores the data. This is a flexible approach which means you can troubleshoot in App Insights, build complex queries in Log Analytics and surface Key Performance Indicators (KPIs) in Azure [Dashboards](https://learn.microsoft.com/en-us/azure/azure-portal/azure-portal-dashboards), all from the same set of data.

In this lab we'll run a few apps, see how to integrate them with Application Insights, and explore the UI for monitoring application health.


## Reference

- [Application Insights overview](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview?tabs=net)

- [Integrating Application Insights with App Service apps](https://learn.microsoft.com/en-us/azure/azure-monitor/app/azure-web-apps)

- [Application Insights SDKs (.NET)](https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core?tabs=netcore6)

- [`az monitor app-insights` commands](https://learn.microsoft.com/en-us/cli/azure/monitor/app-insights?view=azure-cli-latest)

## Create Application Insights

Create a new resouce in the Portal, search for "application insights" and create. For the new resource you can configure:

- the usual resource name and region options
- there is no public DNS for app insights, so the name does not have to be globally unique
- choice of mode - _Classic_ or _Workspace_

There's not much else. The _mode_ is a choice between an older architecture where App Insights owned the data storage, and the latest approach where the data is stored in a Log Analytics Workspace.

Log Analytics is the better approach, because you can store data from several sources in one place and query across them all.

We'll create the Workspace and App Insights in the cli:

```
az group create -n labs-appinsights -l eastus --tags courselabs=azure

az monitor log-analytics workspace create -g labs-appinsights -n labsloganalytics -l eastus

az monitor app-insights component create --app labs --kind web -g labs-appinsights --workspace labsloganalytics -l eastus
```

Browse to the new Application Insights in the Portal. There are lots of interesting-sounding features:

- Application Map
- Live Metrics
- Failures

These won't show anything yet, because we don't have any apps sending data to App Insights. We'll do that next.

## Deploy App with Custom App Insights

In the source folder `src/fulfilment-processor-ai` there is an application which uses the App Insights SDK to send logs and metrics data to AppInsights. There's explicit code in there to record the events we care about. [Worker.cs](/src/fulfilment-processor-ai/Worker.cs) has code like this:

- `telemetry.StartOperation` - records that an _operation_ has started; this is a unit of work with a processing duration
- `telemetry.TrackEvent` - records that an _event_ has happened, with properties to identify the event type and other custom data
- `telemetry.TrackDependency` - records that a _dependency_ has been called, with the duration of the call and the success or failure status

Those are all custom App Insights functions. There is also a lot of logging in the app, using the standard .NET logging framework, which App Insights can also collect.

To connect the app to App Insights you'll need the connection string:

```
az monitor app-insights component show --app labs -g labs-appinsights --query connectionString -o tsv
```

A Docker image for the app is available on Docker Hub. Create some ACI containers configured with your App Insights connection string:

```
# we'll use a new resource group for the apps:

az group create -n labs-appinsights-apps --tags courselabs=azure -l eastus

# start 3 containers running v1.0 of the app:

az container create -g labs-appinsights-apps  --image courselabs/fulfilment-processor:appinsights-1.0 --no-wait --name fp1 --secure-environment-variables "ApplicationInsights__ConnectionString=<appinsights-connection-string>"

az container create -g labs-appinsights-apps  --image courselabs/fulfilment-processor:appinsights-1.0 --no-wait --name fp2 --secure-environment-variables "ApplicationInsights__ConnectionString=<appinsights-connection-string>"

az container create -g labs-appinsights-apps  --image courselabs/fulfilment-processor:appinsights-1.0 --no-wait --name fp3 --secure-environment-variables "ApplicationInsights__ConnectionString=<appinsights-connection-string>"

# and 1 container running v1.2:

az container create -g labs-appinsights-apps  --image courselabs/fulfilment-processor:appinsights-1.2 --no-wait --name fp4 --secure-environment-variables "ApplicationInsights__ConnectionString=<appinsights-connection-string>"
```

> Those containers will all start and be publishing to App Insights within a few minutes

Back to Application Insights in the Portal:

- open the _Live metrics_ view in application insights. How is this app looking?

- what can you find out about this app's dependencies from the _Application map_ view?

- can you see the average time to process a batch from the _Performance_ view? 

This is all powerful stuff, but for a backend app like this we need to write the custom code to capture the metrics we care about. For standard apps running in PaaS, App Insights can do that for us.

## Add Application Insight to a web app

The Random Number Generator is a .NET web app with a backend API. There is some logging in the codebase but no integration with App Insights. If we run those components in App Service we can instrument them without changing the apps.

Start by deploying the website into a new App Service:

```
cd src/rng/Numbers.Web

az webapp up -g labs-appinsights-apps --plan rng-plan-01 --os-type Linux --runtime dotnetcore:6.0 -l westus -n <web-name>
```

You can browse to the app and see the home page, but there won't be any metrics collected by App Insights yet.

Open your Web App in the Portal, browse to _Application Insights_ and click _Turn on Application Insights_:

- choose _Select existing resource_ and choose the Application Insights instance from this lab
- under _Instrument your application_ select _.NET Core_ as the runtime
- click _Apply_

Browse and refresh a few times. Now metrics are being sent to App Insights - Azure knows this is a web application, so it can record information about HTTP requests and responses, and it knows it's .NET so it will collect the application logs. 

Try to get a random number from the app - **it will fail** because the API isn't running yet. That failure will get recorded in App Insights too.

Back in Application Insights head to the _Failures_ view; filter by `Role=<web-name>` and you should see the failed dependency. Click the failure and the _End-to-end transaction details page loads_. Explore these features:

- _Show what happened before and after_
- _Show timeline for this user_

Can you drill down into the error log from these views?

> To fix the app we need to run the API and set the API URL in configuration

That's a REST API so we can get auto-instrumentaion with App Insights for that component too.

## Add a REST API to App Insights

Deploy the Random Number API to the same App Service Plan:

```
cd ../Numbers.Api

az webapp up -g labs-appinsights-apps --plan rng-plan-01 --os-type Linux --runtime dotnetcore:6.0 -l westus -n <api-name>
```

Open the API in the Portal:

- get the URL which we'll need to set in the web app configuration
- add it to _Application Insights_ using the same approach we used for the web app

Then open the web app in the Portal and add two appsettings in the _Configuration_:

- `RngApi__Url` set to your API URL (`https://<your-api/rng`)
- `APPINSIGHTS_JAVASCRIPT_ENABLED` set to `true`

Save the settings - now the web app can use the API, and Application Insights is configured to capture the client experience from the browser, as well as the server metrics.

Try the app now, get a few numbers and see what you get in app insights:

- open _User flows_ and search on the GET `/rng` event to map out the user workflow
- open _Metrics_ and try some graphs 
    - there are client-side metrics under _BROWSER_ 
    - and under _CUSTOM_ you can see the queue size for the fulfilment processor

📋 In your browser open developer tools and look at the network flow when you refresh the page. What extra traffic is there now that `APPINSIGHTS_JAVASCRIPT_ENABLED` is set?

<details>
  <summary>Not sure?</summary>

Client-side library makes a `track` call with a payload like this:

```
[
    {
        "time": "2022-11-16T15:10:51.770Z",
        "iKey": "f5a9b948-cdf7-4763-8b27-da83fcd7d1c5",
        "name": "Microsoft.ApplicationInsights.f5a9b948cdf747638b27da83fcd7d1c5.Pageview",
        "tags": {
            "ai.user.id": "QX+GXnwjWg5rTvOHCLqk+F",
            "ai.session.id": "gF1hYn5gDg2IkjRq83Vz0w",
            "ai.device.id": "browser",
            "ai.device.type": "Browser",
            "ai.operation.name": "/",
            "ai.operation.id": "8fae7b0c2b0b4501ae8c09b3d390e0fe",
            "ai.internal.sdkVersion": "javascript:2.8.9",
            "ai.internal.snippet": "4",
            "ai.internal.sdkSrc": "cdn2"
        },
        "data": {
            "baseType": "PageviewData",
            "baseData": {
                "ver": 2,
                "name": "Courselabs - Numbers.Web",
                "url": "https://clabsazes2211163.azurewebsites.net/",
                "duration": "00:00:00.253",
                "properties": {
                    "refUri": "https://sandbox-8-2.reactblade.portal.azure.net/"
                },
                "measurements": {},
                "id": "8fae7b0c2b0b4501ae8c09b3d390e0fe"
            }
        }
    },
    {
        "time": "2022-11-16T15:10:51.771Z",
        "iKey": "f5a9b948-cdf7-4763-8b27-da83fcd7d1c5",
        "name": "Microsoft.ApplicationInsights.f5a9b948cdf747638b27da83fcd7d1c5.PageviewPerformance",
        "tags": {
            "ai.user.id": "QX+GXnwjWg5rTvOHCLqk+F",
            "ai.session.id": "gF1hYn5gDg2IkjRq83Vz0w",
            "ai.device.id": "browser",
            "ai.device.type": "Browser",
            "ai.operation.name": "/",
            "ai.operation.id": "8fae7b0c2b0b4501ae8c09b3d390e0fe",
            "ai.internal.sdkVersion": "javascript:2.8.9"
        },
        "data": {
            "baseType": "PageviewPerformanceData",
            "baseData": {
                "ver": 2,
                "name": "Courselabs - Numbers.Web",
                "url": "https://clabsazes2211163.azurewebsites.net/",
                "duration": "00:00:00.253",
                "perfTotal": "00:00:00.253",
                "networkConnect": "00:00:00.000",
                "sentRequest": "00:00:00.189",
                "receivedResponse": "00:00:00.001",
                "domProcessing": "00:00:00.063",
                "properties": {},
                "measurements": {
                    "duration": 253.30000000004657
                }
            }
        }
    }
]
```

That gets sent to App Insights to track page load times.

</details><br />

We get all this from Application Insights without any changes to code, because Azure is hosting the application - it's able to inject the JavaScript for the client, and monitor the traffic in the server.

## Lab

Azure Functions also has automatic instrumentation. If you create Functions Apps, Azure creates a separate Application Insights app for each one. In this lab we've created one App Insights instance which we're using for multiple components. Which scenarios do the different approaches support?

> Not sure? Try my [suggestions](suggestions.md).


## Cleanup 


We can delete the Resource Group where the apps are running:

```
az group delete -y --no-wait -n labs-appinsights-apps
```

**If you are moving on to the [Log Analytics lab](/labs//loganalytics/README.md) next, then keep the App Insights RG from this lab**.

Otherwise you can delete that too:

```
az group delete -y --no-wait -n labs-appinsights
```
