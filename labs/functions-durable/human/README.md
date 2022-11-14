

## Reference

- [Durable Functions for Human Interaction](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp#human)


## Pre-req

Set up a Twilio account - for sending SMS messages:

https://www.twilio.com/try-twilio

(You will need to verify with your email address & mobile number, but you won't need to put in credit card details)

And create a phone number (the charge for this will come from your free credits)

> img

Grab the details from your account:

- SID for your Twilio account
- Auth token for your Twilio account



# Reference



func init 2FA --dotnet 

cd 2FA

func new --name Authenticate --template "HttpTrigger"

dotnet add package Microsoft.Azure.WebJobs.Extensions.DurableTask --version 2.8.1

dotnet add package  Microsoft.Azure.WebJobs.Extensions.Twilio --version 3.0.2


## Run locally

- local.settings.json

Start the Azure Storage emulator:

```
docker run -d -p 10000:10000 -p 10001:10001 -p 10002:10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

Run (maybe change the timer to every 1 minute)

```
func start
```

