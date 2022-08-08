
## Reference

- [App Service continuous deployment from GitHub](https://docs.microsoft.com/en-us/azure/app-service/scripts/cli-continuous-deployment-github)

## App deploy from GitHub

- rg, app plan, app svc, deployment --manual-integration


```
az group create -n labs-appservice-cicd -l westeurope --tags courselabs=azure

# minimum Standard tier for slots:
az appservice plan create -g labs-appservice-cicd -n app-plan-01 --is-linux --sku S1 --number-of-workers 2
```

- fork repo in GH

```
git remote add labs-appservice-cicd <your-gh-url>

git push labs-appservice-cicd
```

create app &  deployment settings:

```
az webapp create -g labs-appservice-cicd --plan app-plan-01 --runtime dotnetcore:6.0 -n labs-appservice-cicd-es # <dns-unique-name>

az webapp config appsettings set --settings PROJECT='src/rng/Numbers.Api/Numbers.Api.csproj' -g labs-appservice-cicd -n labs-appservice-cicd-es # <dns-unique-name>
```

create first manual deployment - public repo so no need for auth:

```
git remote -v # to check your GH repo URL

az webapp deployment source config -g labs-appservice-cicd --manual-integration --branch main -n labs-appservice-cicd-es --repo-url https://github.com/sixeyed/azure.git 
```

> Check Deployment Center in Portal - _Settings_ show GH config and _logs_ show deployment

curl https://labs-appservice-cicd-es.azurewebsites.net/rng

> update rng min to 1000 and max 10000 in src/rng/Numbers.Api/appsettings.json

``` 
git add src/rng/Numbers.Api/appsettings.json

git commit -m 'Change RNG range'

git push labs-appservice-cicd
```

- check in portal - no new deployment; click _Sync_ in Deployment Center to trigger update

when complete - will show new range:

curl https://labs-appservice-cicd-es.azurewebsites.net/rng

## Configure CI/CD

- PAT token

Generate a PAT from https://github.com/settings/tokens; add _workflow_ & `admin:repo_hook` permissions; copy the token

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

```
az webapp deployment source delete -g labs-appservice-cicd -n labs-appservice-cicd-es

az webapp deployment source config -g labs-appservice-cicd --branch main -n labs-appservice-cicd-es --repo-url https://github.com/sixeyed/azure.git --git-token $token
```

- check in deployment center - UI shows GH user, org and repo
- change RNG range to 1-100

``` 
git add src/rng/Numbers.Api/appsettings.json

git commit -m 'Revert RNG range'

git push labs-appservice-cicd
```


## Add a staging deployment

az webapp deployment slot create --slot staging -g labs-appservice-cicd -n labs-appservice-cicd-es

az webapp config appsettings set --slot staging --settings PROJECT='src/rng/Numbers.Api/Numbers.Api.csproj' -g labs-appservice-cicd -n labs-appservice-cicd-es


> browse to slots in Portal - staging has separate URL, no deployment configured

- edit range again, now 500-5000

git checkout -b staging

git add src/rng/Numbers.Api/appsettings.json

git commit -m 'Update RNG range 500-5000'

git push labs-appservice-cicd staging

> does not trigger a prod update

az webapp deployment source config -g labs-appservice-cicd --branch staging --slot staging -n labs-appservice-cicd-es --repo-url https://github.com/sixeyed/azure.git --git-token $token

Now check in portal - staging slot is deploying

curl https://labs-appservice-cicd-es-staging.azurewebsites.net/rng

curl https://labs-appservice-cicd-es.azurewebsites.net/rng

Swap the slots

az webapp deployment slot swap --slot staging --target-slot production -g labs-appservice-cicd -n labs-appservice-cicd-es

## Lab

Use app settings to set RNG range in slots and swap again