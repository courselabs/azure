# API Management: API Changes & Versioning

APIs are meant to be explicitly defined, so you typically include a versioning scheme in your API spec from the beginning. Then if you need to make a breaking change, you can do it under a new version, and when all clients have migrated to the new one you can retire the previous version.

API Management has versioning support built in, for both breaking and non-breaking changes. These work very nicely with _deployment slots_ in Azure App Services, so you can run multiple versions of your application code and have APIM route between them using your API version scheme.

In this lab we'll create an API with versioning from the start, and see how APIM and deployment slots let us publish minor revisions and new versions.

## Reference

- [Versioning with APIM](https://azure.microsoft.com/en-gb/blog/api-versioning-with-azure-api-management/)

- [APIM Versions and Revisions](https://azure.microsoft.com/en-gb/blog/versions-revisions/)

- [Staging environments with Deployment Slots](https://learn.microsoft.com/en-us/azure/app-service/deploy-staging-slots)

## Deploy version 1.0 of the API

We'll deploy a few versions of the Random Number Generator API in this lab, and see how to evolve the API specification in APIM.

Start by running a new web app for version 1.0 of the API, using the same Resource Group where your APIM instance is running:

``` 
# switch to the folder with the API source code:
cd src/rng/Numbers.Api

# use a Standard SKU:
az webapp up -g labs-apim --os-type Linux --sku S1 --runtime dotnetcore:6.0 -l westus -n <webapp-name>
```

> The Standard SKU gives us 5 deployment slots

We can use those slots to run different versions of the app at the same time.

Now create a new API in APIM:

- create from OpenAPI spec
- select the _Full_ UI in the window
- check the _Version this API?_ tickbox 
- set the _Version identifier_ to `1.0`
- set the _Versioning scheme_ to _Header_
- set the _Version header_ to `x-api-version`

Upload the spec from the repo in `labs/apim-versioning/rng-v1.0.json`

When your web app is running, set the URL for the app as the Web Service URL in the API (e.g. `https://myrngapi.azurewebsites.net`).

Test the `rng` operation in the APIM designer. Note that the HTTP request specifies the API version 1.0 in the header - **this will be a requirement for consumers**. Versioned APIs need to know which version you're calling.

## Add a revision to version 1.1

A _revision_ is a non-breaking change, like adding optional parameters to an existing operation. Consumers don't need to update because the code change is built to use defaults if the new parameters are not provided.

We'll create a new deployment slot for the 1.1 release so we can dual-run it with the 1.0 release, and test before it goes live. We'll use the naming conventions for blue-green deployments:

```
az webapp deployment slot create -g labs-apim --slot blue -n <webapp-name>
```

Deploy a new version of the RNG API to the blue slot:

```
# run this from the root folder in the repo:
az webapp deployment source config-zip -g labs-apim --src src/rng/Numbers.Api-v1.1/rng-api-v1-1.zip --slot blue -n <web-app-name>
```

Open the web app and check _Deployment Slots_ - you'll see `Production` and `blue`. Click the `blue` link - **this slot has its own URL**.

> That's how we can test the new feature - a revision in APIM will point to the new slot

The original 1.0 deployment will still point to the `Production` slot.

Open the _Revisions_ tab of the RNG API. There is one existing revision; click _Add revision_ and put in a description for the new revision:

```
Now you can set the range of the random number you want, using `min` and `max` parameters in the query string!
```

Note that in the API view above the _Design_ tab it now says _REVISION 2_.

Open the _Settings_ tab and change the Web service URL to your deployment slot URL. Now revision 1 is pointing to the _Production_ slot and revision 2 is pointing to the _blue_ slot. End users never see this URL, so it doesn't matter that it has an odd name.  

In the _Design_ tab, open the rng operation and edit the _Frontend_ to add two query parameters:

- min
- max

Leave `Required` unticked for both.

In the _Test_ tab you can now see the min and max values. Set them to 15 and 45 and test the call; then delete them and test again. You should get a working response whether you include the parameters or not.

> Note that APIM adds the revision to the URL, to distinguish this from the live revision `https://<apim-name>.azure-api.net/rand;rev=2/rng`

Back in the _Revisions_ tab, make Revision 2 the current. Now the normal URL is pointing to the 1.1 release.

Test with curl and you can add the min and max parameters to the original URL. If you don't include them the app still works, so consumers don't need to change.


## Publish version 2.0 of the API

The optional parameters are not ideal because we can't validate them properly. v1.0 clients never expected they might get a 400 response so we can't add that into a revision because it could break their code.

To clean up the API with proper validation and an updated spec, we need a new version.

Start by deploying the v2.0 code to a new deployment slot:

```
az webapp deployment slot create -g labs-apim --slot green -n <webapp-name>

az webapp deployment source config-zip -g labs-apim --src src/rng/Numbers.Api-v2/rng-api-v2-0.zip --slot green -n <web-app-name>
```

Now in APIM create a new version of the API:

- version `2.0`
- versioning scheme `Header`
- Version header `x-api-version`
- Full API version name `rng-api-v2`

Import the OpenAPI spec into the 2.0 version from the file `labs/apim-versioning/rng-v2.0.json`, selecting the _Update_ method.

In the _Settings_ tab for version 2.0 use the URL for your `green` deployment slot as the Web service URL.

Test the v2.0 `rng` operation - min and max are now mandatory, with a 400 response if they are not set.

We can have both versions live, and continue to support 1.1 and 2.0 with revisions and new deployment slots. 

## Lab

Is this really a blue-green deployment? App Service deployment slots can be swapped which does give you the blue-green experience, but is that possible with APIM in front?


> Stuck? Try my [suggestions](suggestions.md).
___

## Cleanup

We're done with APIM now, so you can delete the RG with your APIM instance and all the backends:

```
az group delete -y --no-wait -n labs-apim
```