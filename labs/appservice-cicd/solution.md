# Lab Solution

We need to set two config values in the staging slot:

- Rng__Range__Min=50
- Rng__Range__Max=500

You can do that in the Portal by adding a _New application setting_.

Or in the CLI:

```
az webapp config appsettings set --slot staging --settings Rng__Range__Min=50 -g labs-appservice-cicd -n <dns-name>

az webapp config appsettings set --slot staging --settings Rng__Range__Min=500 -g labs-appservice-cicd -n <dns-name>
```

Compare the _Configuration_ blade for the two slots and you should see that only the staging slot has those settings.

Test the app at the staging URL:

```
curl https://<dns-name>-staging.azurewebsites.net/rng
```

And if it looks good, swap the staging and production slots:

```
az webapp deployment slot swap --slot staging --target-slot production -g labs-appservice-cicd -n <dns-name>
```