# API Managment: Mocking new APIs

New APIs are often designed with a three-way discussion between the API architect, the data owner and the API consumer. That approach ensures you have an API which adheres to best practices and provides the consumer with the information they need from data you know is available.

The period between design and delivery could be long, so it's a good idea to publish a _mock_. This is a real API service which has all the operations agreed in the design, but returns dummy data. Teams can program against the mock until the real API is available.

In this lab we'll use API management to design an API, publish mock responses, and test that it adheres to the agreed specification.

## Reference

- [Mocking API responses](https://learn.microsoft.com/en-us/azure/api-management/mock-api-responses?tabs=azure-portal)

## Create a new API

You should have an existing API Management service from the [API Management lab](/labs/apim/README.md). Browse to it in the Portal and create a new API:

- select _manually defined HTTP API_
- enter any name and URL 
- use the APIM URL suffix `newapi`

Open the _Definitions_ tab - this is where you define the types of object the API works with.

Create a definition called _Student_ from this sample JSON:

```
{
    "StudentId": 2315125,
    "FullName" : "Test One"
}
```

And another definition called _StudentDetail_ from this sample JSON:

```
{
    "StudentId": 2315125,
    "CompanyId": 124121,
    "FirstName" : "Test",
    "LastName" : "Two",
    "Courses" : [
        {
            "CourseCode": "AZDEVACAD",
            "Completed" : "22-11"
        },
        {
            "CourseCode": "K8SFUN",
            "Completed" : "21-01"
        }
    ]
}
```

And lastly an array definition called _StudentArray_  using this **payload**:

```
{
    "type": "array",
    "items": {
        "$ref": "#/definitions/Student"
    }
}
```

The API will let you manage students, using these resources definitions.

## Add mocked operations

Add operations to the API design to list students, create a student, get the details for a student and delete a student.

- _List Students_ 
    - GET from the url `/students`
    - returns `200 OK` response
        - with an `application/json` representation of the `StudentArray` definition

- _Create Student_
    - POST to the url `/students`
    - with a request payload
        - an `application/json` representation of the `StudentDetail` definition
    - returns `201 Created` response
        - with an `application/json` representation of the `StudentDetail` definition
        
- _Get Student_ 
    - GET from the url `/students/{studentId}`
    - with `studentId` as a template parameter
    - returns `200 OK` response
        - with an `application/json` representation of the `StudentDetail` definition
    - returns `404 Not found` response
        - with no payload

- _Delete Student_ 
    - DELETE to the url `/students/{studentId}`
    - with `studentId` as a template parameter
    - returns `204 No Content` response
        - with no payload
    - returns `404 Not found` response
        - with no payload

For each operation:

- add an _Inbound processing_ policy
- use the `mocked-response` policy
- select the correct response code

> Test the operations to check that you get a mocked response with the correct data types

## Publish the mocked API

Add the new API to the _Unlimited_ product, and create a subscription for the product.

Test your API with curl using the subscription key - you should get the mocked responses for each operation:

```
# this should return a student array:
curl "https://<apim-name>.azure-api.net/newapi/students" -H "Ocp-Apim-Subscription-Key: <subscription-key>"

# this should return a student detail:
curl "https://<apim-name>.azure-api.net/newapi/students/1234" -H "Ocp-Apim-Subscription-Key: <subscription-key>"
```

curl is the litmus test for REST APIs - if you can navigate them from the command line, then consumers will definitely be able to work with them in code.

## Consume & Test with Postman

curl isn't very user-friendly though. A great (free) tool for working with REST APIs is Postman:

- [install Postman](https://www.postman.com/downloads/) *or*
- try [Postman online](https://web.postman.co/home)

Postman is about the most popular tool for working with REST APIs. You can set up all the requests you want to make and parameterize any variables, so it's very flexible.

There's a Postman _collection_ in this repo which has the consumer's expectation of the API you've just mocked out. You should be able to import that in Postman, point it to your mock and make all of the operation calls:

- import the collection file `labs/apim-mock/students.postman_collection.json`
- open the collection and navigate to the _Variables_ tab:

![Postman collection variables](/img/postman-collection-variables.png)

Set the values for the mock API you created in APIM:

- `baseUrl` is the full URL e.g. `https://myapim.azure-api.net/newapi`
- `apiKey` is the subscription key - the same one you used with curl 

Click _Save_ and try all the operations - they should all give the expected response code and response. If not you'll need to check your API design in APIM.

## Lab

This API spec was manually put together in the APIM designer. That's easy to do but not easy to share. How could you distribute the API spec to consumers?


> Stuck? Try [suggestions](suggestions.md) 
___

## Cleanup

**Don't clean up yet!** 

One APIM instance can host multiple APIs and we'll use the same resource in the next few labs, rather than deleting it and waiting another hour to create a replacement.