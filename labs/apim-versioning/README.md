# API Management: API Changes & Versioning

breaking & non-breaking change; build versioning scheme into API to start with so clients know what to expect

## Reference

- [Versioning with APIM](https://azure.microsoft.com/en-gb/blog/api-versioning-with-azure-api-management/)

- [APIM Versions and Revisions](https://azure.microsoft.com/en-gb/blog/versions-revisions/)

## Deploy & Publish the v1 API

Run a new web app for the random number API:

``` 
# switch to the folder with the API source code:
cd src/rng/Numbers.Api

# use a Standard SKU:
az webapp up -g labs-apim --os-type Linux --sku S1 --runtime dotnetcore:6.0 -l westus -n <webapp-name>
```

Create a new API in APIM:

- create from OpenAPI spec
- select the _Full_ UI in the window
- check the _Version this API?_ tickbox 
- set the _Version identifier_ to `1.0`
- set the _Versioning scheme_ to _Header_
- set the _Version header_ to `x-api-version`

Upload the spec in `labs/apim-versioning/rng-v1.0.json`

When your web app is deployed, set the URL for the app as the Web Service URL in the API.

Test the `rng` operation. Note that the HTTP request specifies the API version 1.0 in the header.

## Add a revision to v1

A _revision_ is a non-breaking change, e.g. when you add optional parameters to an operation. Existing consumers don't need to update because the code change is built to use defaults if the new parameters are not provided.

We'll use a [deployment slot]() for the new code so we can test before it goes live, using naming conventions for blue-green deployments:

```
az webapp deployment slot create -g labs-apim --slot blue -n <webapp-name>
```

Deploy a new version of the RNG API to the blue slot:

```
# from the root folder in the repo:
az webapp deployment source config-zip -g labs-apim --src src/rng/Numbers.Api-v1.1/rng-api-v1-1.zip --slot blue -n <web-app-name>
```

Open the web app and check _Deployment Slots_ - you'll see `Production` and `blue`. Click the `blue`` link - **this slot has its own URL**.

> That's how we can test the new feature - a revision in APIM will point to the new slot. 

Open the _Revisions_ tab of the RNG API. There is one existing revision; click _Add revision_ and put in a description for the new revision:

```
Now you can set the range of the random number you want, using `min` and `max` parameters in the query string!
```

Note in API view above the _Design_ tab it now says _REVISION 2_.

Open the _Settings_ tab and change the Web service URL to your staging slot URL.

In the _Design_ tab, open the rng operation and edit the Frontend to add two query parameters:

- min
- max

Leave `Required` unticked for both.

In the _Test_ tab you can now see the min and max values. Set them to 15 and 45 and test the call; then delete them and test again. You should get a working response from both.

> Note that APIM adds the revision to the URL, to distinguish this from the live revision `https://fwefwfw.azure-api.net/rand;rev=2/rng`

Back in the _Revisions_ tab, make Revision 2 the current.

Test with curl and you can add the min and max parameters to the original URL. If you don't include them the app still works, so consumers don't need to change.


## Publish the v2 API

The optional parameters are not ideal because we can't validate them properly. v1 clients never expected to have a 400 response so we can't add that into a revision because it would break.

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

Test the v2 rng operation - min and max are now mandatory with a 400 response if they are not set.

We can have both versions live, and continue to support v1 and v2 with revisions and new deployment slots. 

## Lab

Is this really a blue-green deployment? App Service deployment slots can be swapped which does give you the blue-green experience, but is that possible with APIM in front?