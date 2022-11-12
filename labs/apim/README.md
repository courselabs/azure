
## Reference

- mutual TLS between web app & apim https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-mutual-certificates

- apim ip addresses https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-ip-addresses

## Portal

```
az group create -n labs-apim --tags courselabs=azure -l westus
```

- new 'api management'

- org name for portal & email 

- create in `labs-apim` rg (takes 60+ min? start first tthing then do other labs & come back)

- comes with default "echo api"

Open developer portal URL

- 

## Deploy Backend API

- deploy backend - can be function app, ;logic app or web app:

``` 
# switch to the folder with the API source code:
cd src/rng/Numbers.Api

az webapp up -g labs-apim --os-type Linux --sku B1 --runtime dotnetcore:6.0 -l westus -n <webapp-name>
```

browse to the openapi docs published by the api:

`http://<webapp-url>/swagger`

> Standard format for describing APIs

Actual URL for the docs is https://clabsazes221112.azurewebsites.net/swagger/v1/swagger.json

Add that as an API in your APIM.

## Configure the API

APIM takes a lot of exploring.

Configure your API in the Portal such that:

- the /healthz endpoint returns 404
- the /reset endpoint can only be accessed from your own IP address (see https://ifconfig.me)
- the /rng response is cached for 30 sconds

Test each endpoint to validate the policies are working as you expect

```<policies>
    <inbound>
        <base />
        <return-response>
            <set-status code="404" reason="Not found" />
            <set-header name="x-apim" exists-action="override">
                <value />
            </set-header>
            <set-body />
        </return-response>
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>
```

## Pulblish your API and Developer Portal

For the portal:

Add your own company name and UI.

Require an AD account o sign up.

Enable CORS.

Publish the portal.

For the API:

Require consumers to sign up for a subscription (you can use the default `Starter` product)

## Sign up as a customer

Use a private browser session to open your developer portal and sign up for an account - you need to provide a real email address and password.

You'll get a verification email with a link to click/

> All this is default potrtal logic, hosted in the APIM service

Log in and explore the portal. Find the rng API call and test it - it will fail. You need a subscription key.

For that you need to browse to products and add a _Starter_ subscription.

You'll bet a subscription key. Now you can try the /rng API.

Can you call the API with curl?

Test page shows you curl, e.g.

```
curl -v -X GET "https://fwefwfw.azure-api.net/numbers/rng" -H "Cache-Control: no-cache" -H "Ocp-Apim-Subscription-Key: 1c54e95364de429c8e780b3865f3e57a"
```

Try repeatedly - should get the same number within 30 seconds and limited to 5 calls per minute

Too many and you will get:

```
{ "statusCode": 429, "message": "Rate limit is exceeded. Try again in 51 seconds." }
```

## Lab

Customize email text (notification templates); automate deployment (save to repo & clone); is the original Web APp url still available?


Verify that the API requires a token and the  rules above are applied