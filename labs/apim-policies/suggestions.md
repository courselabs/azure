# Lab Suggestions

No, APIM only listens on published operations. If you send in a request which is not known to APIM it doesn't get forwarded on to the backend, the client gets a 404 Not found response.

That might seem like a bad thing, because it's not at all flexible - you need to explicitly model every operation. Actually that's a good thing :) APIs are meant to have a fixed contract, you should be able to fetch the specification and be confident it describes everything the API can do.
