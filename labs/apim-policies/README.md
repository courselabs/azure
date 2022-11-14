# API Managment: Request and Response Policies

already used caching & mocking

## Reference

- [Protecting your API](https://learn.microsoft.com/en-us/azure/api-management/transform-api)

## Explore the backend API

https://swapi.dev

> Yes you can front  3rd party API as long as you have access

Make a GET request in Postman:

```
https://swapi.dev/api/people/3
```

Inspect the response and you'll see two issues:

- the `Server` header has the value `nginx` which we don't want to expose
- the response body contains lots of links back to the original API, which we don't want to use

We can fix both of those with response policies.

Also SWAPI has its own rate limiting, restricting calls by IP address. That's usually fine - one end-user is unlikely to max out the limit. But all the calls will come from one APIM IP address so we want to cache the responses and not pass all the load onto SWAPI.

## Create the API in API Managment

Create a new HTTP API with the Web service URL `https://swapi.dev/api`.

Add one operation:

-  GET `/people/{personId}`
- returns 200 OK JSON response 
- with an `application/json` response
- the respone entity will be like this sample:

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



## Configure Header & Cache Policies

Add three policies to the operation which will help secure the API:

- an inbound policy to use response caching - set the cache duration to `86400` seconds - a full day

- an outbound policy to remove the `Server` response header, and instead write a custom `x-server` header the the value `swapi-apim`

- an outbound policy to replace all SWAPI URLs in the response body with your own APIM URLs, so the links still work but the origin is not shown

## Publish & test the API

Add the SWAPI API to one of your products and test it using Postman (or `curl -v`):

- you should get a 401 error unless you present a subscription key in the header
- authorized requests should get a valid response
- URLs in the response should use your domain not swapi.dev
- the response header should have your `Server` value

```
curl "https://fwefwfw.azure-api.net/swapi/people/14" -H "Ocp-Apim-Subscription-Key: f83f4f70fb4f48c5943e0113d1273ff6" #<subscription-key>"

```

## Lab

The response from SWAPI is pretty good REST. There are URLs linking the entity to other entities, e.g. people/3 has a species field which refers you to species/2. Can you follow those links from your own APIM wrapper for SWAPI?
