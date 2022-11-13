To package the source ZIP file for Kudu deployment; ensure the `.deployment` file is correct. 

```
# from /src/rng/Numbers.Api-v2

zip -r ../rng-api-v2-0.zip . *
```


Reference: https://learn.microsoft.com/en-us/cli/azure/webapp/deployment/source?view=azure-cli-latest#az-webapp-deployment-source-config-zip