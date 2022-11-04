FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src
COPY asset-manager.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out asset-manager.csproj

# app image
FROM mcr.microsoft.com/dotnet/aspnet:6.0

ENV Database__Api="BlobStorage" \
    KeyVault__Enabled="false" \
    LockFile__Enabled="true" \
    LockFile__Path="/app/lockfiles" \
    ConnectionStrings__AssetsDb="TODO"

ENTRYPOINT ["dotnet", "/app/asset-manager.dll"]

WORKDIR /app
COPY --from=builder /out/ .