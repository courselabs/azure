# Lab Solution

Create a new directory for the lab:

```
mkdir ../lab 

cd ../lab
```

Run `func new` and follow the prompts:

- choose PowerShell
- choose HTTP trigger
- give the trigger a name (e.g. "hello")

Start the function:

```
func start
```

If you're running in a PowerShell session then the function will run and you can try it out:

```
curl http://localhost:7071/api/hello?name=courselabs
```

The function will fail to run locally if you don't have PowerShell - you need to pick a runtime which you have installed.

But you can still deploy to Azure.

Try to use the same function app:

```
func azure functionapp publish <function-name>
```

> This will fail because all the functions in one Function App need to use the same language runtime, and the existing function is .NET

Remember the hosting plan in the consumption model plan is just a placeholder, with no servers and no cost. You can create a new Function App in the same region and it will use the same plan:

```
az functionapp create -g labs-functions-http  --runtime powershell --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name-2> 
```

Now you can publish:

```
func azure functionapp publish <function-name-2>
```

> There's no compilation step this time, as it's a scripted function

This should publish and runs fine. Check in the Portal - the function defaults to needing auth, and the _Get function URL_ button gets you the key.