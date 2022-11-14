To package the source ZIP file for Kudu deployment; ensure the [.deployment]() file is correct. 

```
# from /src/rng/Numbers.Api-v1.1

zip -r ../rng-api-v1-1.zip . *
```


Reference: https://learn.microsoft.com/en-us/cli/azure/webapp/deployment/source?view=azure-cli-latest#az-webapp-deployment-source-config-zip