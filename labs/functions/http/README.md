# Azure Functions

Functions are a _serverless_ compute platform - you supply the code and you don't need to provision infrastructure or manage scale. Functions get _triggered_ in response to events, which could be a file being uploaded in Blob storage or an incoming HTTP request.

Azure Functions are hosted in a Functions App which is a type of App Service (like Web Apps). Function Apps are part of an App Service Plan but they can use a _consumption_ model, which means infrastructure is only provisioned and paid for while functions are running, there is no cost when there's no activity.

In this lab we'll get started with a simple function triggered from an HTTP call, and see how to run it locally and in Azure.

## References

- [Azure Functions overview](https://learn.microsoft.com/en-us/azure/azure-functions/functions-overview)

- [Functions developer guide](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference?source=recommendations&tabs=blob)

- [`az functionapp` commands](https://learn.microsoft.com/en-us/cli/azure/functionapp?view=azure-cli-latest)

- [`func` CLI commands](https://learn.microsoft.com/en-us/azure/azure-functions/functions-run-local?tabs=v4%2Cmacos%2Ccsharp%2Cportal%2Cbash#create-a-local-functions-project)

## Explore Azure Functions

Open the Portal and create a new resource. Search for 'function', select _Function app_ and _Create_. There are some interesting options:

- the function app name is a DNS prefix
- deployment choice is Code or Docker Container
- for Code you need to choose runtime & OS
- the consumption plan is serverless, but you can use an always-on plan with Premium
- a Storage Account is required, this is where the Function App stores logs and other details

> Go ahead and create the new Function App, creating a new RG and Storage Account to use

If you create a Function App in the Portal, you can edit code directly in the browser. Open _Functions_ from the left menu and click _Create_:

- select _Develop in Portal_ from the dropdown
- choose the _HTTP Trigger_ template from the list
- call the new function _hello_
- set the _Authorization level_ to _Anonymous_

When the function is created, switch to the _Code + Test_ menu. Can you run your function from here? Edit the response the function sends back and save it. Browse to the URL - can you call the function from the browser?

You can delete the RG for the Portal function when you've finished exploring.


## Functions Command Line

The Portal is great for experimenting, but for anything other than a playground we'll want function code in source control. Azure Functions have a custom CLI you use to create projects, run functions on your local machine and deploy to Azure.

First we need to install the Azure Functions Core Tools (using [Homebrew](https://brew.sh) on the Mac or [Chocolatey](https://chocolatey.org/install#install-step2) on Windows):

```
# Windows:
choco install azure-functions-core-tools

# macOS:
brew tap azure/functions
brew install azure-functions-core-tools@4
```

> There are [other installers available](https://github.com/Azure/azure-functions-core-tools) - be sure to install the v4 option for your OS.

Check your installation:

```
func --version
```

The output should show you're on version 4 (mine is 4.0.4829 but yours might be newer).

## Run Functions locally

There is a .NET function library with an HTTP trigger in the `calendar` folder:

- [calendar/HttpTime.cs](/labs/functions/http/calendar/HttpTime.cs) - just writes a log entry and sends a response with the current time

You can use the `func` CLI to run the function locally:

```
cd labs/functions/http/calendar

func start
```

> You'll see output from the .NET build, and then a line saying the function is listening on localhost

Try it:

```
curl http://localhost:7071/api/HttpTime
```

This is great for a short feedback loop. You can test the functionality without waiting for an Azure deployment, make changes and run `func start` again to test them before you go live.

That one command is all you need to run functions locally :) Next we'll deploy to Azure.

## Deploy to Azure Functions

In Azure we'll need to create the pre-requisites - a Resource Group and Storage Account:

```
# find a location near you which supports serverless functions:
az functionapp list-consumption-locations -o table

# create the RG:
az group create -n labs-functions-http --tags courselabs=azure -l eastus

# and storage account - use the region you want for your function:
az storage account create -g labs-functions-http --sku Standard_LRS -l eastus -n <sa-name>
```

📋 Create a  Function App to host the function, using the `az functionapp create` command. Choose .NET as the runtime and Functions version 4. Select your region for the consumption-plan and link it to the Storage Account you just created.

<details>
  <summary>Not sure how?</summary>

Check the help text:
```
az functionapp create --help
```

The flag for the region is different from usual, because you specify the consumption plan and location in one:

```
az functionapp create -g labs-functions-http  --runtime dotnet --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name> 
```

</details><br/>

Check in the Portal - you've got an app service plan with no scale up or scale out options, this is the serverless (consumption) plan. You can see the function app:

- has a public URL
- uses Windows, which is the default OS
- has a similar UX to Web Apps, because they're all App Service features

Now you can deploy from the functions CLI direct to Azure:

```
func azure functionapp publish <function-name>
```

> You'll see the app compiled locally and uploaded

The output includes the public URL for the function. This one uses anonymous authorization, so you can test it with:

```
curl <public-url>/api/HttpTime
```

Check the storage account in the Portal. You'll see blob containers are locked, but file storage is available to view. This is the root filesystem for the app; can you find the application logs?

## Add more functions

One Function App can host multiple functions - typically you group together all the functions for a project into one Function App.

We have some more functions we can add in the `update` folder:

- [update/HttpDate.cs](/labs/functions/http/update/HttpDate.cs) - HTTP trigger which responds with the current date

- [update/HttpDay.cs](/labs/functions/http/update/HttpDay.cs) - HTTP trigger which responds with the current day of the week

```
# check you're still in the calendar folder:
pwd

# copy the new functions to this folder:
cp ../update/*.cs .

# start the function emulator:
func start
```

> Now the new functions are in the project folder, you'll see three HTTP functions listed

The name of the function in the code becomes the URL path to call for an HTTP trigger.

Test the new functions:

```
curl http://localhost:7071/api/HttpDate

curl http://localhost:7071/api/HttpDay
```

📋 Deploy the updated functions to Azure.

<details>
  <summary>Not sure how?</summary>

The new functions are in your local project, so it's the same publish command:

```
func azure functionapp publish <function-name>
```

</details><br/>

Try some of the functions when they're deployed:

```
curl <public-url>/api/HttpTime

curl <public-url>/api/HttpDate
```

Check in the Portal - you still have one Function App in the Plan. Open it and you can see the individual _Functions_. Open one and check the menu options - _Integration_ shows you the triggers, inputs and outputs; _Monitor_ shows you the recent runs and results, click one to see the logs.

Try the other new function:

```
curl -i <public-url>/api/HttpDay
```

> Youll get a _Not authorized_ error because this function requires authentication

📋 Browse the function options in the Portal to find an authentication key, so you can call the `HttpDay` function with curl.

<details>
  <summary>Not sure how?</summary>

Back in the Portal open the function click _Get function URL_ - here the output includes a key, also shown in _Function Keys_. The Portal adds the key because the function is set with authorization required.

Your URL with the token will look something like this:
```
curl https://courselabsazes.azurewebsites.net/api/HttpDay?code=UUGtEqCqMqF-whsqFzfaafajkURTOgXqVS1lZ9eepgHIXvObgAzFuJ2bGVA==
```

</details><br/>

The structure of functions is the same as other App Services:

- App Service Plan -> App Service(s) -> App(s)
- App Service Plan -> Function App(s) -> Function(s)

The big difference with functions is that the App Service Plan can be on the consumpion model, so you don't need to have any servers running.

## Lab

Your turn to try this out. Create a new directory and from there create a new function with `func new`. Choose Powershell as the runtime and pick the HTTP trigger template. Run the function locally - does it work without any code changes? Publish the function to Azure. Can you use your existing Function App to host the new function?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the lab RG:

```
az group delete -y --no-wait -n labs-functions-http
```