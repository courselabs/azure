# API Managment: Request and Response Policies

Policies are the plug-in features in API Management that let you change the behaviour of API operations. Input policies fire before the backend gets called and can alter the request or avoid calling the backend at all. Output policies fire after the backend has been called and can alter the response before it gets sent to the client.

We've already used a few policies - for caching and sending mocked responses - but there are some other policies you should always look to add, because they increase the security of your APIs.

In this lab we'll present a public API through APIM, using policies to enhance security.

## Reference

- [Protecting your API](https://learn.microsoft.com/en-us/azure/api-management/transform-api)

## Explore the backend API

There's a public API popular with developers called SWAPI - the Star Wars API. You can read about it at https://swapi.dev, it's a REST API which returns information about characters, places and other data types from the Star Wars films.

We can expose the public API through our APIM instance.

> Yes, you can front third-party APIs, as long as their usage policy lets you do that

Open [Postman](https://www.postman.com/downloads/) and make a GET request for one of the SWAPI resources:

```
https://swapi.dev/api/people/3
```

Inspect the response and you'll see there are two potential security issues:

- the `Server` header has the value `nginx` which we don't want to expose
- the response body contains lots of links back to the original API, which we don't want people to see

These are issues because attackers like to know what server software sent the response, so they can see if there are any known exploits in that version of the software. 

Also we want to keep traffic within APIM so we know our policies are being applied; if the responses contain links to the original server then attackers can bypass APIM and go straight to the source.

We can fix both of those with response policies.

SWAPI is a public service too, and we should respect its bandwidth. The API has its own rate limiting, restricting calls by IP address. That's usually fine - one end-user is unlikely to max out the limit. But all the calls will come from one APIM IP address so we want to cache the responses and not pass all the load onto SWAPI.

## Create the API in API Managment

Add a new _manually defined_ HTTP API in APIM with the Web service URL `https://swapi.dev/api`.

Add one operation:

- GET `/people/{personId}`
	- returns `200 OK` response 
	- with an `application/json` response
	- the response entity will be like this sample:

```
{
	"name": "R2-D2",
	"height": "96",
	"mass": "32",
	"hair_color": "n/a",
	"skin_color": "white, blue",
	"eye_color": "red",
	"birth_year": "33BBY",
	"gender": "n/a",
	"homeworld": "https://swapi.dev/api/planets/8/",
	"films": [
		"https://swapi.dev/api/films/1/",
		"https://swapi.dev/api/films/2/",
		"https://swapi.dev/api/films/3/",
		"https://swapi.dev/api/films/4/",
		"https://swapi.dev/api/films/5/",
		"https://swapi.dev/api/films/6/"
	],
	"species": [
		"https://swapi.dev/api/species/2/"
	],
	"vehicles": [],
	"starships": [],
	"created": "2014-12-10T15:11:50.376000Z",
	"edited": "2014-12-20T21:17:50.311000Z",
	"url": "https://swapi.dev/api/people/3/"
}
```

There are other resource types in SWAPI, but we're only going to expose the _people_ through our APIM.

## Configure Header & Cache Policies

Add three policies to the operation which will help secure the API:

- an inbound policy to use response caching - set the cache duration to `86400` seconds (a full day)

- an outbound policy to remove the `Server` response header

- an outbound policy to write a custom `x-server` response header with the value `swapi-apim`

- an outbound policy to replace all SWAPI URLs in the response body with your own APIM URLs, so the links still work but the origin is not shown

📋 Test the operation through the designer to validate the policies are working as you expect.

<details>
  <summary>Need some help?</summary>

Response caching and header manipulation are standard policies which you can find in the operation UI.

Replacing all the URLs in the response will need an entry in _other policies_ - that's the XML view, and there's a snippet which does what you want. Be sure to add the snippet in the right place in the XML.

</details><br/>

## Publish & test the API

Add the SWAPI API to one of your APIM Products and test the `/people` operation using Postman (or `curl -v`):

- you should get a 401 error unless you present a subscription key in the header
- authorized requests should get a valid response
- URLs in the response should use your APIM domain and not `swapi.dev`
- the response header should have your `x-server` value and not the original `Server` header

```
# e.g.
curl -v "https://<apim-name>.azure-api.net/swapi/people/14" -H "Ocp-Apim-Subscription-Key: <subscription-key>"
```

## Lab

The response from SWAPI is pretty good REST. There are URLs linking the entity to other entities, e.g. `people/3` has a _species_ field which refers you to `species/2`. Can you follow those links from your own APIM wrapper for SWAPI? Is that a good or a bad thing?


> Stuck? Try [suggestions](suggestions.md) 
___

## Cleanup

**Don't clean up yet!** 

One APIM instance can host multiple APIs and we'll use the same resource in the next lab, rather than deleting it and waiting another hour to create a replacement.