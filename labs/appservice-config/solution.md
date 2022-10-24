# Lab Solution

**The UX for this is not friendly.** 

In the Portal open the App Service and choose _Diagnose & solve problems_ and then _Diagnostic Tools_.

Click on _Auto-Heal_, enable it and add a rule:

- if 1 request has a status code of 500
- in a 30-second window
- recycle the process.

Save the settings and break the app by making multiple calls for random numbers. Check the /healthz endpoint when both instances are failed and you'll see they get replaced with new ones within a few minutes.

Here's my output showing failed instances being replaced:

```
PS>curl https://rng-api-es2.azurewebsites.net/healthz
{"message":"Instance: f08cd58fd50f. Unhealthy"}
PS>curl https://rng-api-es2.azurewebsites.net/healthz
{"message":"Instance: 325b6b19d360. Unhealthy"}
PS>curl https://rng-api-es2.azurewebsites.net/healthz
Instance: c45fb7b5cd08. Ok
PS>curl https://rng-api-es2.azurewebsites.net/healthz
Instance: c45fb7b5cd08. Ok
PS>curl https://rng-api-es2.azurewebsites.net/healthz
Instance: a6a7a06dc0d1. Ok
```