# Log Analyics


## Reference

- [Log Analytics Overview]()
- [Kusto Query Language](https://learn.microsoft.com/en-us/azure/data-explorer/kusto/query/)


## Run log-generating apps

> see appinsights lab,. run containers with appinsights ingestion to loga

## Query Application Logs

_Logs_ view, switch to _Tables_

simplest query is table name; try:

- AppEvents
- AppDependencies
- AppTraces

Traces are the actual log entries written by the application.

```
AppTraces
| limit 100

AppTraces
| distinct SeverityLevel

AppTraces
| summarize LogsBySeverity = count() by(SeverityLevel)
```

Do you have any severity 1 or 0 logs? Can you find the individual log entries for a specific severity level - print just the message, application name and instance name.


You can make your query easier to work with by using variables:

```
let sev=3;
AppTraces
| where SeverityLevel == sev
| project TimeGenerated, SeverityLevel, Message, AppRoleName, AppRoleInstance
```

- How many logs of sev 3 have the error code 302?


## Aggregating Application Metrics

Everything is recorded as a log, which means you use the same query language to summarize metrics and drill into individual log entries.

```
AppMetrics
| count

AppMetrics
| limit 10

AppMetrics
| where (Name == "QueueSize")
| summarize AvgQueueLength = avg(Sum)
```


More useful - restrict time period and track queue size over time:

```
AppMetrics
| where (Name == "QueueSize")
| where (TimeGenerated > ago(2hours))
| summarize AvgQueueLength = avg(Sum) by bin(TimeGenerated, 10m)
```

More useful still - render that as a graph:

```
AppMetrics
| where (Name == "QueueSize")
| where (TimeGenerated > ago(2hours))
| summarize AvgQueueLength = avg(Sum) by bin(TimeGenerated, 10m)
| render timechart
```

> Average queue length in 10-minute buckets, from the last 2 hours

## Lab

Dashboard