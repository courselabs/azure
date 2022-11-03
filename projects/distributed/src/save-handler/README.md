To package the deployment ZIP file for Azure App Service WebJob:

```
# from /src/save-handler directory
dotnet publish --configuration Release -o .deploy/app_data/Jobs/Continuous/SaveHandler

cp run.cmd .deploy/app_data/Jobs/Continuous/SaveHandler

cd .deploy ; zip -r ../deploy.zip . * ; cd ..
```


Reference: https://github.com/MicrosoftDocs/azure-docs/issues/24121