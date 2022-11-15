# API Management

API Management (APIM) is a full service for delivering HTTP APIs to consumers - it could be a public API or an external API for business partners. API Management gives you four core features: an API designer where you can describe and version your API's operations; a gateway which routes incoming traffic to backend API hosts and can modify requests and responses; a developer portal where consumers can on-board themselves as users of the API; integrated security to restrict API access.

It's a very powerful service with lots more features which we'll explore. In this lab we'll go through the basics of APIM; creating an instance of the service can take 60+ minutes, so you might want to do that in advance.

## Reference

- [API Management overview](https://learn.microsoft.com/en-us/azure/api-management/api-management-key-concepts)

- [Mutual TLS between Web Apps & APIM](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-mutual-certificates)

- [Virtual IP address for APIM](https://learn.microsoft.com/en-us/azure/api-management/api-management-howto-ip-addresses)

- [`az apim` commands](https://learn.microsoft.com/en-us/cli/azure/apim?view=azure-cli-latest)

## Create a new API Management resource and explore

Creating APIM takes a long time, so we'll create through the CLI:

```
az group create -n labs-apim --tags courselabs=azure -l westus

az apim create --no-wait --sku-name Developer -g labs-apim -n <apim> --publisher-name <company-name> --publisher-email <real-email-address> 
```

> Developer tier is cheap to run (currently $0.07/hr) and is fine to use for exploration - there's no SLA so you can't use it for real environments

You'll get an email when your APIM instance is ready to use. Then you can open it in the Portal:

- in the overview there's the gateway URL, management URL and developer portal URL
- the left nav has _APIs_, _Products_ and _Subscriptions_

You can host multiple APIs in one APIM resource. _Products_ are the business unit that users can sign up for - a product gives them access to one or more APIs. _Subscription_ will show the users who have signed up for the products.

## Deploy Backend API

APIM isn't a hosting service for APIs, you need to deploy your actual API logic to another service inside (or outside) Azure. You'll add it as a backend in APIM so the service knows where to route incoming calls.

We'll use a web app as an API backend - it could also be a Function App, Logic App or a custom URL.

Deploy the random number generator API as a Web App:

``` 
# switch to the folder with the API source code:
cd src/rng/Numbers.Api

az webapp up -g labs-apim --os-type Linux --sku B1 --runtime dotnetcore:6.0 -l westus -n <webapp-name>
```

That API hosts its own documentation, containing the specification of the API. Browse to the documentation at `http://<webapp-url>/swagger`

> Swagger is an open source tool for REST API documentation. 

It also publishes the documentation in JSON using the standard [OpenAPI specification](https://www.openapis.org). You can see the JSON spec at `https://<webapp-name>.azurewebsites.net/swagger/v1/swagger.json`.

APIM can import OpenAPI specs:

- open the _APIs_ blade in the portal
- click to _Add API_
- select _Import from_ _OpenAPI_
- enter the URL for your web app's `swagger.json`
- add a name and display name

The API will open in the designer, and now we can explore some more of APIM.

## Configure the API

APIM takes a lot of exploring. One of the main features is the ability to add inbound and outbound processing policies, to wrap the actual logic of the API.

Configure your Random Number Generator API such that:

- the `/rng` operation response is cached for 30 sconds - this reduces the load on the actual API
- the `/reset` operation can only be accessed from your own IP address (see https://ifconfig.me to see your public IP address) - this is an administrative function which should be restricted
- the `/healthz` operation always returns 404 - this is an internal health check endpoint and shouldn't be public

📋 Test each endpoint through the designer to validate the policies are working as you expect.

<details>
  <summary>Need some help?</summary>

You'll need to add an _input processing_ policy to each operation. Filtering for IP addresses and caching responses are standard policies you should be able to find in the UI. 

Returning a custom response instead of calling the API will need an entry in _other policies_ - that's an XML view which looks nasty, but there are snippets to add the feature we need. Be sure to add the snippet in the right place in the XML.

</details><br/>

Right now the API isn't public, which is fine for testing the initial setup. When you're happy with the configuration, you can publish the API.

## Publish your API and Developer Portal

There are a few steps to publishing the API.

The API itself needs to be configured in _Settings_:

- the _web service URL_ needs to be the URL of your backend web app
- the API needs to be added to a _Product_ to make it available
- APIM has two default products, you can use one of them or create your own

The _Developer Portal_ link opens the designer view:

- add your own company name 
- personalize the UI

Then you can configure the Developer Portal under _Portal overview_:

- allow use of an AD account to sign up
- enable CORS
- Publish the portal

Now your Developer Portal is available, new users can sign up and use the API.

## Sign up as a customer

Use a private browser session to open your Developer Portal and sign up for an account - you need to provide a real email address and strong password. You'll get a verification email with a link to follow.

> All this is default potrtal logic, hosted in the APIM service

Log in with your user and explore the Developer Portal. 

Find the Random Number Generator API call and try it with the test page - it will fail. You need a subscription key.

For that you need to browse to products and add a subscription (users can choose from the products which you have enabled for the API e.g. _Starter_ or _Unlimited_).

Now you'll get a subscription key which you can use to call the `/rng` operation.

📋 Can you call the API with curl?

<details>
  <summary>Need some help?</summary>

The test page shows different options for calling the API, including curl. The command will include your subscription key, like this:

```
curl "https://<apim-name>.net/<api-name>/rng" -H "Ocp-Apim-Subscription-Key: <suscription-key>"
```

</details><br />

Try to call the endpoint repeatedly. You should get the same random number every time within a 30-second period, if you set up your cache correctly. 

Different products have different limits, if you used the _Starter_ product you are limited to 5 calls per minute. Exceed that and you will get an error response:

```
{ "statusCode": 429, "message": "Rate limit is exceeded. Try again in 51 seconds." }
```

> These are production-grade features that you get without having to write any code

That's why APIM is so powerful. You can focus your API on your business logic and leave infrastructure concerns to APIM.

## Lab

We've only touched the capabilities of APIM, but even so there are some questions which need thinking about:

- can you customize the text of emails that get sent to users signing up?
- all the clicking and pointing is error prone, how would you automate APIM configuration?
- endpoint policies are all applied by APIM, but is your backend web app still publicly available?


> Stuck? Try [suggestions](suggestions.md) 
___

## Cleanup

**Don't clean up yet!** 

One APIM instance can host multiple APIs and we'll use the same resource in the next few labs, rather than deleting it and waiting another hour to create a replacement.
