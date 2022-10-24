# Lab Solution

If you don't have a fork of the repo in GitHub, create one [here](https://github.com/courselabs/azure/fork).

Make sure you update the [index.html](static/index.html) file to use your own API URL. The easiest way to do that is to edit it directly in GitHub and commit the changes there.

Then deploy a static web app from your fork:

```
az staticwebapp create  -g labs-appservice-api --branch main --app-location "/labs/appservice-api/static" --login-with-github -l westeurope -n <dns-name> --source <github-fork-url>
```

The app will take a few minutes to deploy - you can check the status of the GitHub Action on your fork. 

When it's done, browse to the app, try and get a random number and you will see the CORS issue.

You can add an _allowed origin_ domain in the _CORS_ section of the API App Service in the Portal.

Or do it with the CLI:

```
az webapp cors add --allowed-origins 'https://<static-web-fqdn>'  -g labs-appservice-api -n <api-app-service>
```

> It will take a few minutes for the rule to be added, then you can call the RNG API from the static web app:

![SPA RNG App](/img/rng-spa.png)