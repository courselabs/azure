# Lab Suggestions

The orchestrator in a durable function can call other activities at any point in the workflow, and those activity functions can use all the usual input and output bindings. You could add an activity which posted a message to a queue when the user has sent in the authentication code.

Then the web application just listens on the queue, and can log the user in when the authentication message confirms they have got the authentication code correct.