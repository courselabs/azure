FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src
COPY src/queue-worker.csproj .
RUN dotnet restore

COPY src/ .
RUN dotnet publish -c Release -o /out queue-worker.csproj

# app image
FROM mcr.microsoft.com/dotnet/runtime:6.0

ENTRYPOINT ["dotnet", "/app/queue-worker.dll"]

WORKDIR /app
COPY --from=builder /out/ .
COPY appsettings.json .