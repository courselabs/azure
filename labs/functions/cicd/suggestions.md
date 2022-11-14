# Lab Suggestions

Function chaining is about setting up a workflow, and you need to carefully think about the inputs and outputs you need, and whether the steps need to run in sequence, or if they can be in parallel and potentially in a different order than you intended.

There is no trigger functionality for Table Storage, so we couldn't use the output of the WriteLog function as the trigger for the NotifySubscribers function. If we needed that then one option is to use CosmosDB instead for the log, as we could trigger the notification from that. 

Using blob storage purely to trigger other functions is a reasonable pattern - it means you get persistent storage of whatever data happened in the original event, and it's cheap to use. If the following activities have no dependencies, then they can all use the same blob as a trigger, and run independently.