## Functions: Blob Storage to SQL Server

triggers invoke the function; bindings for input and output

## Reference

- [Blob storage trigger & binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp)

- [SQL Server binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-azure-sql?tabs=in-process%2Cextensionv3&pivots=programming-language-csharp)

- [SQL Server binding samples](https://github.com/Azure/azure-functions-sql-extension)

## Scheduled Function writing to Blob Storage

expolore func cs

- [TimerToBlob/Hearbeat.cs](labs/functions/timer/TimerToBlob/Hearbeat.cs)

For reference:

```
func init BlobToSql --dotnet 

cd BlobToSql

func templates list

func new --name UploadLog --template "Azure Blob Storage trigger"

dotnet add package Microsoft.Azure.WebJobs.Extensions.Storage.Blobs --version 5.0.0
```

## Test the function locally

Start the Azure Storage emulator:

```
docker run -d -p 10000:10000 -p 10001:10001 --name azurite mcr.microsoft.com/azure-storage/azurite
```

Start SQL Server in an ACI container:

```
az group create -n labs-functions-blob

az container create -g labs-functions-blob --name mssql --image mcr.microsoft.com/mssql/server:2019-CU14-ubuntu-20.04 --ports 1433 --environment-variables "ACCEPT_EULA=Y"  "MSSQL_PID=Express" --secure-environment-variables "MSSQL_SA_PASSWORD=AzureD3v!!!" --memory 3 --dns-name-label <dns-name>
```

Connect to the container:

```
az container exec -g labs-functions-blob --name mssql --exec-command "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P AzureD3v!!!"
```

Create the database schema:

```
CREATE DATABASE func
GO

USE func
GO

CREATE TABLE dbo.UploadLogItems (Id uniqueidentifier primary key, BlobName nvarchar(200) not null, Size int null)
GO

SELECT * FROM UploadLogItems
GO
```

Ctrl-c

create file local.settings.json with:

mssqllablbobes.westeurope.azurecontainer.io

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "UploadInputStorageConnectionString": "UseDevelopmentStorage=true",
        "UploadSqlServerConnectionString": "Data Source=<sql-container-dns-name>;Initial Catalog=func;Integrated Security=False;User Id=sa;Password=AzureD3v!!!;MultipleActiveResultSets=True"
    }
}
```

Run

```
func start
```

In another terminal:

```
az storage container create --connection-string 'DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;' -n uploads

az storage blob upload --connection-string 'DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;' --file labs/functions/blob/document.txt --container uploads --name document1.txt
```

> Should see output in function:

```
[2022-11-07T17:11:19.263Z] New blob uploaded:document1.txt
[2022-11-07T17:11:20.643Z] Stored blob upload item in SQL Server
[2022-11-07T17:11:20.653Z] Executed 'UploadLog' (Succeeded, Id=986759b8-a91e-4a0a-a8ec-694cf315f972, Duration=1415ms)
```

Check in SQL:

```
az container exec -g labs-functions-blob --name mssql --exec-command "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P AzureD3v!!!"

USE func
GO

SELECT * FROM UploadLogItems
GO
```

Ctrl-c

## Deploy to Azure

Setup:

```
az group create -n labs-functions-blob --tags courselabs=azure -l eastus

az storage account create -g labs-functions-blob --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-blob  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

Pre-reqs:

- a separate storage account for input, with a blob container `uploads`
- the connection string for the SA set as appsetting `UploadInputStorageConnectionString`

- a SQL Azure instance with the database scheme deployed as above
- the connection string for SQL set as appsetting `UploadSqlServerConnectionString`

```
func azure functionapp publish <function-name>
```

## Lab

How would you automate the SQL schema creation?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Stop the Azure Storage emulator:

```
docker rm -f azurite
```

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-blob
```
