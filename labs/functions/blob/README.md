# Functions: Blob Storage to SQL Server

Functions are very good as integration components - gluing together systems which don't have any way of being connected directly. _Data-level integration_ is one option where a function is triggered when one system writes data, it reads that data and adapts or enriched it before writing it to another system.

In this lab we'll use a function which is triggered from a write to blob storage and generates output by writing a table to SQL Server. We'll test the function locally with containers for the dependencies and then deploy to Azure.

## Reference

- [Blob storage trigger & binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-blob-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp)

- [SQL Server binding reference](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-azure-sql?tabs=in-process%2Cextensionv3&pivots=programming-language-csharp)

- [SQL Server binding samples](https://github.com/Azure/azure-functions-sql-extension)

## Blob Storage function writing to SQL Server

The function code is in the `BlobToSql` directory:

- [BlobToSql/UploadLog.cs](/labs/functions/blob/BlobToSql/UploadLog.cs) - executes when a blob is stored and writes a record to SQL Server

These attibutes take care of the trigger and binding:

- `[BlobTrigger]` sets the function to run when a blob is created in the `uploads` container
- `[Sql]` is an output binding which will create a record in the database table `UploadLogItems`

<details>
  <summary>For reference</summary>

Here's how the function was created:

```
func init BlobToSql --dotnet 

cd BlobToSql

func new --name UploadLog --template BlobTrigger

dotnet add package Microsoft.Azure.WebJobs.Extensions.Sql
```

</details><br/>

The trigger provides key details about the blob which has been uploaded - the name and the full content. The SQL Server binding specifies a table name and connection string; it takes care of mapping an object to the database schema, but the table needs to exist before the function runs.

## Test the function locally

Start the Azure Storage emulator and SQL Server database in containers:

- [docker-compose.yml](/labs/functions/blob/docker-compose.yml) - defines containers for each dependency

```
docker compose -f labs/functions/blob/docker-compose.yml up -d
```

When they are ready you can connect to SQL Server and create the database. We'll use the command line here but you can use any SQL client (connect to `localhost:1433` with the username `sa` and the password in the Compose file).

Connect to the container:

```
docker exec -it blob-mssql-1 "/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P AzureD3v!!!"
```

> If you're using an Arm64 machine (e.g. Apple Silicon) then the database engine runs fine but the Docker image doesn't have the SQL tools installed. If you see an error _no such file or directory_ then you'll need to use a SQL client like [SqlEctron](TODO).

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

Exit the container shell session with Ctrl-C/Cmd-C.

Create the configuration file `labs/functions/blob/BlobToSql/local.settings.json` with this content:

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "UploadInputStorageConnectionString": "UseDevelopmentStorage=true",
        "UploadSqlServerConnectionString": "Data Source=localhost;Initial Catalog=func;Integrated Security=False;User Id=sa;Password=AzureD3v!!!;MultipleActiveResultSets=True"
    }
}
```

Now all the dependencies and configuration are in place, you can run the function locally:

```
cd labs/functions/blob/BlobToSql

func start
```

> You'll see the host output with the single `blobTrigger` function listed

In another terminal you can create a blob container and upload a file to the storage emulator - **this is the correct account key** - which is hard-coded in the emulator:

```
az storage container create --connection-string 'DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;' -n uploads

az storage blob upload --connection-string 'DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;' --file labs/functions/blob/document.txt --container uploads --name document1.txt
```

You should see output lines like this in the function:

```
[2022-11-07T17:11:19.263Z] New blob uploaded:document1.txt
[2022-11-07T17:11:20.643Z] Stored blob upload item in SQL Server
[2022-11-07T17:11:20.653Z] Executed 'UploadLog' (Succeeded, Id=986759b8-a91e-4a0a-a8ec-694cf315f972, Duration=1415ms)
```

Connect to your SQL Server container again and check the data is there:

```
USE func
GO

SELECT * FROM UploadLogItems
GO
```

Over to you for the Azure deployment.

## Deploy to Azure

Here's the basic function setup to get you going:

```
az group create -n labs-functions-blob --tags courselabs=azure -l eastus

az storage account create -g labs-functions-blob --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-blob  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

Now you'll need the pre-reqs for the function:

- a storage account for input with a blob container called `uploads`
- the connection string for this storage account set as appsetting `UploadInputStorageConnectionString`

- a SQL Azure instance with the database scheme deployed as above (you can use the database explorer in the Portal for that)
- the connection string for SQL set as appsetting `UploadSqlServerConnectionString`
- the Function App will need network access to SQL Server

When you have those running you can deploy the function:

```
func azure functionapp publish <function-name>
```

And test it by uploading some files to blob storage.

## Lab

How would you automate the SQL schema creation?

> Stuck? Try [suggestions](suggestions.md).

___

## Cleanup

Stop the Docker containers:

```
docker compose -f ../docker-compose.yml down
```

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-blob
```
