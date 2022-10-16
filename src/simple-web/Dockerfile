FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

WORKDIR /src
COPY src/simple-web.csproj .
RUN dotnet restore

COPY src/ .
RUN dotnet publish -c Release -o /out simple-web.csproj

# app image
FROM mcr.microsoft.com/dotnet/aspnet:6.0

ENV App__Environment="DEV"

ENTRYPOINT ["dotnet", "/app/simple-web.dll"]

WORKDIR /app
COPY --from=builder /out/ .