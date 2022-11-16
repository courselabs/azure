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

## Deploy Insights-Enabled App

- fulfilment processor to ACI
- set app insights cs

https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core?tabs=netcore6

## Application Logs

## Telemetry