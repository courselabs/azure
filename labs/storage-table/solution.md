

In the portal - open _Storage browser_ to table, click _Advanced filters_. Then you can filter by the _Level_ property:

```
Level eq 'Error'
```

Or with REST - you need a  SAS token for the new table and you need to encode the query:

```
$expiry=$(Get-Date -Date (Get-Date).AddHours(1) -UFormat +%Y-%m-%dT%H:%MZ)

az storage table generate-sas -n FulfilmentLogs --permissions r --expiry $expiry -o tsv --account-name labsstoragetablees

curl -H 'Accept: application/json;odata=nometadata' 'https://labsstoragetablees.table.core.windows.net/FulfilmentLogs()?$filter=Level%20eq%20%27Error%27&se=2022-08-10T15%3A40Z&sp=r&sv=2019-02-02&tn=FulfilmentLogs&sig=PYlrhbvzYPqEm53StCk8nZx5yIl0bf0GlrMabEZRxPw%3D'
```

You can also use $select to return just the log column:

```
curl -H 'Accept: application/json;odata=nometadata' 'https://labsstoragetablees.table.core.windows.net/FulfilmentLogs()?$filter=Level%20eq%20%27Error%27&$select=RenderedMessage&se=2022-08-10T15%3A40Z&sp=r&sv=2019-02-02&tn=FulfilmentLogs&sig=PYlrhbvzYPqEm53StCk8nZx5yIl0bf0GlrMabEZRxPw%3D'

```