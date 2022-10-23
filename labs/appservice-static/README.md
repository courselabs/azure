# App Service for Static Web Apps

App Service does a great job of compiling, packaing and deploying web apps - but there's another option for simpler apps. If you have a static HTML site or a Single Page Application (SPA) with no backend processing, you can deploy that as a Static Web App. Content for a static web app is loaded from an external Git repository, so you can't deploy from the local filesystem.

In this lab we'll see how to create a static web app from a GitHub repo, and also compare it to a standard web app with static content.


## Reference

- [Azure Static Web Apps overview](https://learn.microsoft.com/en-us/azure/static-web-apps/overview)

- [App Service Plan overview](https://docs.microsoft.com/en-us/azure/app-service/overview-hosting-plans)

- [`az appservice` commands](https://docs.microsoft.com/en-us/cli/azure/appservice?view=azure-cli-latest)

- [`az staticwebapp` commands](https://learn.microsoft.com/en-us/cli/azure/staticwebapp?view=azure-cli-latest)


## Create a Static Web App

You'll need a GitHub account for this exercise (they're free - you can [sign up here](https://github.com/signup)). Log in and browse to create a fork of the course repo: 

- https://github.com/courselabs/azure/fork

Confim by clicking _Create fork_:

![GitHub fork page](/img/github-fork.png)

> When that completes you'll have your own copy of the course labs in GitHub under your own username.

Add your fork as a new remote, which means you can make changes and push them to your own copy of the repo. Copy the URL for your fork from the GitHub page (the default URL will be https://github.com/<github-username>/azure). Use that for your remote:

```
git remote add fork <github-fork-url>
```

This is the repo your static web app will be deployed from.

📋 Create a Resource Group and use the `staticwebapp create` command to deploy a new static web app from your repo. You'll need to use the `main` branch, and the location ofthe application content is `/labs/appservice-static/html`.

<details>
  <summary>Not sure how?</summary>

The RG is easy - use your own choice of location:

```
az group create -n labs-appservice-static  -l westeurope --tags courselabs=azure
```

Check the help text for creating a static web app:

```
az staticwebapp create --help
```

There's a nice option to login interactively with GitHub, so you don't need to create an access token:

```
az staticwebapp create  -g labs-appservice-static --branch main --app-location "/labs/appservice-static/html" --login-with-github -n labsappservicestatices --source <github-fork-url>
```

</details><br/>

If you use interactive GitHub authentication, the CLI will print a secret code and a web page will launch asking you to provide the code and confirm access from Azure.

When that completes, open the Portal and explore the resources in the RG:

- there's just a static web app resource - no app service or plan
- open the resource and you'll see it has a public URL
- browse to the URL and confirm you can see the site

## Push Content Changes

The static web app also has a link to the deployment workflow - open that and you'll see a YAML file in GitHub. This is a GitHub Action which Azure created and added to your fork of the repo. It runs every time you push changes to your fork.

Edit the HTML content in this file:

- [html/index.html](labs/appservice-static/html/index.html)

📋 Using the git command line, fetch from the fork to sync the workflow file, then add your changes, commit them and push to the fork.

<details>
  <summary>Not sure how?</summary>

```
git fetch fork main

git add labs/appservice-static/html/index.html

git commit -m 'Update static web app'

git push fork main
```

</details><br/>

Open your fork in GitHub - 


cd labs/appservice-static/html 

az webapp up -g labs-appservice-static --html --sku F1 -n <unique-dns-name> 

> creates app service plan - random name; ZIP deploys from current dir

browse to output - it will take a while to come online

OPen the web app in the portal and check the console - what OS and web server is it hosted on?

Use curl to check how the content is delivered:

curl -IL <url>


> Windows, ASP.NET - default for static web sites; SSL provided with Azure domain name

etag & last modified set automatically & help with caching

Check local FS - .azure folder contains config file for future deployments

## Using Node for Static content


cd ../node 

az webapp up -g labs-appservice-static --os-type Linux --runtime node:16-lts --sku B1 -n <unique-dns-name> 

> new app service plan; requested Linux, current is Windows


Compare scaling options for app service plans

Compare app services - console is SSH for linux

Browse to site & check with vurl:

curl -IL <url>

## Authentication & Authorization

Portal for Node app
 
 Click _Add identity provider_
 - Select Microsoft as the IdP
 - Select _Any Azure AD directory & personal Microsoft accounts_

Open a new private browser window and browse to your Node js URL

> It may take a few minutes for auth to be configured; you'll see a login page when it is

No changes to the app; log in with Azure account - you'll be asked to confirm the app can read account details

browse to /user on the site - you'll see Azure listed as the IdP and your Azure Account email address

## Lab

check the logs for both apps - browse to some non-existent URLs and see how the 404s are reported

(Kudu - IIS web logs for static; node app - docker logs in Log Stream)
