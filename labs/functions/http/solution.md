
cd ../lab

func new

chose powershell

chose HTTP trigger

name hello

func start

curl http://localhost:7071/api/hello?name=courselabs

> Will fail if you pick a runtime which you don't have installed

Can still deploy to same function app:

```
func azure functionapp publish <function-name>
```

> will fail if the runtime is different

Consumption model plan is just a placeholder - same region will use same plan:

```
az functionapp create -g labs-functions  --runtime powershell --functions-version 4 --consumption-plan-location eastus --storage-account <sa-name> -n <function-name-2> 
```

Now publish:

```
func azure functionapp publish <function-name-2>
```

> No compilation, scripted

Publishes and runs OK. Check in the Portal - defaults to needing auth, _get function url_ contains key. Can see code in Portal but not edit because of deploy method.