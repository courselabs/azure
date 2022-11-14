FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src
COPY src/logging/ToDoList.Logging.csproj ./logging/
COPY src/entities/ToDoList.Entities.csproj ./entities/
COPY src/messaging/ToDoList.Messaging.csproj ./messaging/
COPY src/model/ToDoList.Model.csproj ./model/
COPY src/web/ToDoList.csproj ./web/

WORKDIR /src/web
RUN dotnet restore

COPY src/ /src
RUN dotnet publish -c Release -o /out ToDoList.csproj

# app image
FROM mcr.microsoft.com/dotnet/aspnet:6.0

WORKDIR /app
ENTRYPOINT ["dotnet", "ToDoList.dll"]

# elevated for access to Sqlite db
ENV DATA_DIRECTORY="/data"\
    USER="root"

ENV ConfigController__Enabled="false"

VOLUME $DATA_DIRECTORY
USER $USER

COPY --from=builder /out/ .