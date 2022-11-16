# Lab Solution

There's a sample workbook here, exported as a JSON template:

- [loganalytics/Lab.workbook)](/labs/loganalytics/Lab.workbook)

The relevant KQL queries are:

_Show most recent timestamp by app instance, displaying time-ago in minutes_

```
AppTraces
| summarize arg_max(TimeGenerated, AppRoleInstance) by AppRoleInstance
| project Instance=AppRoleInstance, LastSeen=datetime_diff('minute', now(), TimeGenerated)
```

_Show count of failures by instance_
```
AppTraces
| where Properties.EventType == "Fulfilment.Failed"
| summarize count() by AppRoleInstance
```

_Show breakdown of queue size over time_
```
AppMetrics
| where (Name == "QueueSize")
| summarize AvgQueueLength = avg(Sum) by bin(TimeGenerated, 10m)
```

You can import that into a new workbook:

- in edit mode click the _Advanced editor_ icon which looks like this: `</>`
- copy and paste the template JSON into the advanced editor

Your workbook should load:

![LogAnalytics Workbook](/img/loganalytics-workbook.png)

