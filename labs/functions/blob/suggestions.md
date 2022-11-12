# Lab Suggestions

SQL automation doesn't have a great story in Azure at the moment. You could create a BACPAC file from a local copy of the database and import it - but that can take a long time.

Alternatively you could keep your schema in source code with a SQL Server project and build it into a DACPAC in your pipeline. Then you can use a tool like SqlPackage (part of SQL Server) to generate a deployment script (like [this example](https://github.com/sixeyed/presentations/blob/master/docker-cambridge/2018-08-ci-cd-database-powered-by-containers/demo3/v1/Initialize-Database.ps1)).