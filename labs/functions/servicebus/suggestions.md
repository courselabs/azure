# Lab Suggestions

"Scale" means different things in messaging solutions. It could be lots of messages coming in a short timeframe, or a sustained rate of messages over a long timeframe. 

Either way you could write a function with a timer trigger - which can be set to fire as regularly as every second - and a Service Bus output binding. The output can be a collection of messages, so your function could generate hundreds of messages per second if you needed to test that level of performance.