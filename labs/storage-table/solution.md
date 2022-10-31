# Lab Solution

## Using the Portal

Open _Storage browser_ and navigate to the table. 

Click _Advanced filters_ then you can filter by the _Level_ property:

```
Level eq 'Error'
```

## Using OData

Yo'll need a  SAS token for the new table:
```
$expiry=$(Get-Date -Date (Get-Date).AddHours(1) -UFormat +%Y-%m-%dT%H:%MZ)

az storage table generate-sas -n FulfilmentLogs --permissions r --expiry $expiry -o tsv --account-name <sa-name>
```

You'll us a filter with the same query as in the Portal `Level eq 'Error'` - but because it goes into the URL you need to [encode it](https://www.w3schools.com/html/html_urlencode.asp); so the actual query string in the URL will be:

```
FulfilmentLogs()?$filter=Level%20eq%20%27Error%27
```

Your full query would look like this, with your own domain and SAS token:

```
curl -H 'Accept: application/json;odata=nometadata' "https://labsstoragetablees.table.core.windows.net/FulfilmentLogs()?$filter=Level%20eq%20%27Error%27&se=2022-10-27T19%3A51Z&sp=r&sv=2019-02-02&tn=FulfilmentLogs&sig=EdZElUPinN2RDnbjiNrzZNfm49LLE/F6st0dJj5bLjQ%3D"
```
