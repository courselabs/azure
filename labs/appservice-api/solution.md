



From repo root:

cd labs/appservice-api/node

az webapp up -g labs-appservice-api --plan app-plan-01 --os-type Linux --runtime node:16-lts -n rng-web-spa-es # dns name 

> Browse, see issue


```
az webapp cors add --allowed-origins 'https://rng-web-spa-es.azurewebsites.net'  -g labs-appservice-api -n rng-api-es
```