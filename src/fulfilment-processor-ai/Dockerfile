FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS builder

WORKDIR /src
COPY fulfilment-processor-ai.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /out fulfilment-processor-ai.csproj

# app image
FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine

ENTRYPOINT ["dotnet", "/app/fulfilment-processor-ai.dll"]

WORKDIR /app
COPY --from=builder /out/ .