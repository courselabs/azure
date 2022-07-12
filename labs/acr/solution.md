# Lab Solution

If you want to automate this, you'll need to start by listing all the repositories in the registry:

```
az acr repository list -n <acr-name>
```

Then you'll loop through the result and list the tags for each repository; the command can sort the output by the date the tag was pushed, e.g:

```
az acr repository show-tags -n <acr-name> --repository labs-acr/simple-web --output tsv --orderby time_desc
```

Then you can iterate through the list, skipping the first 5 - because they're the newest ones which you want to keep. For the rest, build the full image name and then remove it:

```
az acr repository delete --name $acrName --image $imageName --yes --only-show-errors
```

There's a sample script in PowerShell here: [prune-acr.ps1](./lab/prune-acr.ps1).