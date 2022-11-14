

# API Managment: Mocking for new APIs with

3-way design with data owner, consumer and API team; deploy draft API ASAP to allow dev and iterate on design


## Reference

- [Mocking API responses](https://learn.microsoft.com/en-us/azure/api-management/mock-api-responses?tabs=azure-portal)

## Create a new API

IN your existing API Management service, create a new API:

- manually defined HTTP
- any name and URL - use the APIM URL suffix `newapi`

Use _Definitions_ to define object types.

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

And an array definition called _StudentArray_  using this **payload**:

```
{
    "type": "array",
    "items": {
        "$ref": "#/definitions/Student"
    }
}
```

The API will let you manage students, using this definition and the student ID.

## Add mocked operations

Add operations to the API design to list students, create a student, get the details for a student and delete a student.

- _List Students_ 
    - GET from the url /students
    - returns 200 OK response
        - with an `application/json` representation of the `StudentArray` definition

- _Create Student_
    - POST from the url /students
    - with a request payload
        - an `application/json` representation of the `StudentDetail` definition
    - returns 201 Created response
        - with an `application/json` representation of the `StudentDetail` definition
        
- _Get Student_ 
    - GET from the url /students/{studentId}
    - with `studentId` as a template parameter
    - returns 200 OK response
        - with an `application/json` representation of the `StudentDetail` definition
    - returns 404 Not found response
        - with no payload

- _Delete Student_ 
    - DELETE from the url /students/{studentId}
    - with `studentId` as a template parameter
    - returns 204 No Content response
        - with no payload
    - returns 404 Not found response
        - with no payload

For each operation:

- add an _Inbound processing_ policy
- use the `mocked-response` policy
- select the correct response code

> Test the operations to check that you get a mocked response with the correct data types

## Publish the mocked API

Add the new API to the _Unlimited_ product, and create a subscription for the product.

Test your API with curl using the subscription key - you should get the mocked responses

```
# e.g.
curl "https://<apim-name>.azure-api.net/newapi/students" -H "Ocp-Apim-Subscription-Key: <subscription-key>"
```

## Consume & Test with Postman

Install Postman

Import collection labs/apim-mock/students.postman_collection.json

OPen variables page and set URL & API key in _Current_:

Save and try all the operations - they should all give the expected response code and response.

## Lab

How can you share the API spec with a third party?