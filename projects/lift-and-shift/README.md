
## Application Architecture


## Goals

- test environment, small sql server, single app instance
- prod environment, larger sql server, multiple app instances
- automated in scripts: one for creating the whole environment, one for removing it

## Dev Environment

```
dotnet run
```

- browse to http://localhost:5000
- uses Sqlite database
- can add & remove
- check diagnostics page

## Configuration 

appsettings.json

- Database:Provider can be Sqlite or SqlServer
- ConnectionStrings:ToDoDb is the main database connection string
- ConfigController:Enabled - if set to `true` the app will expose a /config page which you can use to check the loaded config

## Testing

- browse to URL - no errors
- add item; check appears in list
- open SQL Server - make sure data stored in SQL
