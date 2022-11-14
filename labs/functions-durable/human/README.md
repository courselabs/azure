# Durable Functions: Human Interaction

An advantage of durable functions is that they can wait for an extended period of time for an activity to complete, and if that activity contains sensitive data it's not stored outside of the function so it's much harder for attackers to get to it. This is perfect for human interaction, where a workflow executes up to a point and then stops, waiting for human input. This lets you build a fully automated workflow but with a human approval step.

In this lab we'll use a durable function with the Twilio binding, to send an SMS text message to a user. The function waits for the user to reply before continuing.

## Reference

- [Durable Functions for Human Interaction](https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp#human)

- [Twilio function binding](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-twilio?tabs=in-process%2Cfunctionsv2&pivots=programming-language-csharp)

## Pre-requisites

Twilio is a service for sending SMS messages. It has a free tier with an allowance which is plenty for development and prototyping. There are a few setup steps we need to do:

> Start by setting up a Twilio account - https://www.twilio.com/try-twilio

You will need to use:

- a real email address - this gets validated
- your real mobile number - this too
- but you won't need to put in any payment details.

From your account page open _API keys & tokens_ and make a note of your authentication details - **keep these secure**:

- SID for your Twilio account
- Auth token for your Twilio account

![Twilio auth token](/img/twilio-auth-token.png)

Now you'll need to create a Twilio phone number - this is the number that will show as the sender when you send messages from Azure functions.

- enter _buy a number_ in the Jump To box in the top menu
- untick all the _Capabilities_ except `SMS` and click _Search_
- choose any number you like the look of and click _Buy_

> The charge for this will come from your free credits

![Twilio buy number](/img/twilio-buy-number.png)

Make a note of your new number and you're ready to go!


## HTTP Trigger with Orchestration

The scenario is a simple two-factor authentication, where a text message gets sent to the use with a code and they need to input the code to continue. We'll just use the standard HTTP calls we get with the orchestration trigger, but these can easily be wrapped up in a nice web UI.

The code is in the `2FA` folder:

- [2FA/Authenticate.cs](/labs/functions-durable/human/2FA/Authenticate.cs) - uses an HTTP trigger which expects a POST with the phone number to verify as a parameter in the URL

- [2FA/SmsVerify.cs](/labs/functions-durable/human/2FA/SmsVerify.cs) - the orchestrator which calls the Twilio activity to send the SMS message and starts a timer; the user needs to reply within 2 minutes or the authorization will fail

- [2FA/SmsChallenge.cs](labs/functions-durable/human/2FA/SmsChallenge.cs) - the activity function which actually calls Twilio, generating a random code for the user and sending the SMS message

Twilio is only used to send the message to a phone number. The function needs to be sent a status update with the user's response code, and it is the function logic which determines if the user is authenticated.

## Test the function locally

There are no dependencies for this function other than the standard Storage Account.

Run Docker Desktop and start the Azure Storage emulator:

```
docker run -d -p 10000-10002:10000-10002 --name azurite mcr.microsoft.com/azure-storage/azurite
```

You will need the local configuration file with your Twilio details, so create a text file at `labs/functions-durable/human/2FA/local.settings.json` and add the standard settings. **Use [E.164 formatting](https://support.twilio.com/hc/en-us/articles/223183008-Formatting-International-Phone-Numbers) - so the number starts with a plus sign, then the country code, then the number - e.g. +447412972480**.

```
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "TwilioAccountSid": "<your-twilio-account-SID>",
        "TwilioAuthToken": "<your-twilio-auth-token>",
        "TwilioPhoneNumber": "<the-twilio-phone-number-you-bought>"
    }
}
```

> Make sure you don't acidentally push the JSON file to GitHub. Twilio have bots monitoring all public repos, and if they find authentication details anywhere they'll regenerate your auth token and send you an email of shame.

Run the function locally:

```
cd labs/functions-durable/human/2FA

func start
```

You'll see the usual startup logs, with the functions listed. 

Make an HTTP POST request to start the workflow - **use your own mobile number** so Twilio will send the SMS message from the number you bought to your own number **and use E.164 formatting**.

E.g. the UK international dialling code is 44, so if my number was 07654 123123 I would use `+447654123123`:

```
# use `curl.exe` on Windows
curl -XPOST http://localhost:7071/api/Authenticate?number=+447654123123
```

You should get a text message with a four-digit code (how exciting!), and you should see logs like this:

```
[2022-11-14T16:13:49.544Z] Starting SmsChallenge for: + 44xxx
[2022-11-14T16:13:49.549Z] Executed 'SmsVerify' (Succeeded, Id=8feb5637-523c-46c4-9fae-262beab6da05, Duration=14ms)
[2022-11-14T16:13:49.586Z] Executing 'SmsChallenge' (Reason='(null)', Id=3d9aa8a2-8c3a-4ef0-807f-6f853e44c90c)
[2022-11-14T16:13:49.587Z] Sending verification code 4091 to + 44xxx.
```

Now you need to confirm the code you got by sending a status update to the function. The response from your curl request to the HTTP trigger included a field with a URL for raising events to the orchestration instance:

```
"sendEventPostUri":"http://localhost:7071/runtime/webhooks/durabletask/instances/eb9fa85442254eb8af7de25efaca5dda/raiseEvent/{eventName}?taskHub=TestHubName&connection=Storage&code=_umg_d2m6RKVVzHbDKM9xmWQjIkhVazcg01c5nKIlMxGAzFulTbm8Q=="
```

Use that URL with the `eventName` set to `SmsChallengeResponse` to make a curl request to confirm your code from the SMS message:

```
# use curl.exe on Windows
curl -XPOST -d <your-sms-code>  -H 'Content-Type: application/json' "http://localhost:7071/runtime/webhooks/durabletask/instances/<id-from-url>/raiseEvent/SmsChallengeResponse?taskHub=TestHubName&connection=Storage&code=<code-from-url>"
```

> This is a bit fiddly and it may take a few goes to get the syntax right. You have five minutes :)

You will see in the function logs whether your authentication code is correct, or if you don't respond in time:

```
[2022-11-14T16:13:50.184Z] Executed 'SmsVerify' (Succeeded, Id=eda56e78-a907-496f-81b3-fab326cd0785, Duration=6ms)
[2022-11-14T16:14:25.583Z] Executing 'SmsVerify' (Reason='(null)', Id=51088b64-54f3-40b7-9069-61b48ea596a8)
[2022-11-14T16:14:25.584Z] Starting SmsChallenge for: + 44xxx
[2022-11-14T16:14:25.585Z] Authorized! User responded correctly to SmsChallenge for: + 44xxx
```

You can also call the URL in the `statusQueryGetUri` field to check on the status. The output will read `true` if you authenticated correctly.

It's worth deploying this in Azure so you can test it out, but you may be disappointed with the UX for working with the orchestration :)

## Deploy to Azure

This is the setup for your Function App:

```
az group create -n labs-functions-durable-human --tags courselabs=azure -l eastus

az storage account create -g labs-functions-durable-human --sku Standard_LRS -l eastus -n <sa-name>

az functionapp create -g labs-functions-durable-human  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

You will need to set your Twilio details in the Function App Settings (use the same values from your local settings JSON):

- `TwilioAccountSid`
- `TwilioAuthToken`
- `TwilioPhoneNumber` 

**There are no dependencies** - no external services are used for triggers or bindings, so you can go right ahead and deploy:

```
func azure functionapp publish <function-name>
```

Try the function by using the test feature in the HTTP trigger - you'll need to add your phone number as a parameter. The response includes the usual URLs to check on the status and send an event, but there is nothing helpful in the Portal to invoke those calls, you still need to build up the URL and use curl.

## Lab

You need to use the HTTP trigger for human interaction functions so that you get the status workflow where consumers can post events - like when the user has entered their code. Ordinarily that would all be taken care of in your web UI, but how can you design it so that the website doesn't need to keep polling the status endpoint to see if the user is authroized?

> Stuck? Try my [suggestions](suggestions.md) 
___

## Cleanup

Stop the Azure Storage emulator:

```
docker rm -f azurite
```

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-durable-human
```
