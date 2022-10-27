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

Add your fork as a new remote, which means you can make changes and push them to your own copy of the repo. Copy the URL for your fork from the GitHub page (the default URL will be `https://github.com/<github-username>/azure`). Use that for your remote:

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
az staticwebapp create  -g labs-appservice-static --branch main --app-location '/labs/appservice-static/html' --login-with-github -n labsappservicestatices --source <github-fork-url>
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

- [html/index.html](/labs/appservice-static/html/index.html)

📋 Using the git command line, pull from the fork to sync the workflow file, then add your changes, commit them and push to the fork.

<details>
  <summary>Not sure how?</summary>

```
git pull fork main

git add labs/appservice-static/html/index.html

git commit -m 'Update static web app'

git push fork main
```

</details><br/>

Open your fork in GitHub - browse to the _Actions_ tab and you'll see a new workflow run which is deploying your updated web content. You can drill in and see all the logs. 

When the deployment completes, refresh your web app page to see the changes.


## Using Web Apps for Static Content

Static web apps are incredibly simple to use, scale well and integrate with other Azure services if you want to couple your app to a backend API. 

You can also use standard web apps for static content, if you need more control over deployment and management options.

Browse to the path on your local machine with the static content, and create it as a web app with the `webapp up` command:

```
cd labs/appservice-static/html 

az webapp up -g labs-appservice-static --html --sku F1 -n <unique-dns-name> 
```

> You'll see the CLI create an App Service Plan and a Web App, generates a ZIP file of the content in the current directory and deploys it.

 - random name; ZIP deploys from current dir

Browse to the URL in the ouput - it's the same static app with your update, but it comes from your local folder so you don't need to commit and push any changes to GitHub.

Open the RG again in the Portal - you'll see the new App Service Plan and App Service. Open the Web App and you'll see the full range of management options, even though this app has no runtime. What OS and web server is the site hosted on?

Use curl to check how the content is delivered:

```
curl.exe -IL <url>
```

> You'll see the web server is IIS so this must be a Window server. It actually uses ASP.NET - which is the default for static web sites using App Service

## Using Node.js for Mixed Content

If you have some static content and some that needs backend processing, you could deliver that with a Static Web App and some serverless functions. Existing apps would need modifying to support that architecture - or you could deploy them as App Service apps.

This lab has a Node.js app which publishes two pieces of content:

- a static HTML page [index.html](/labs/appservice-static/node/public/index.html)
- a /user endpoint in [app.js](/labs/appservice-static/node/app.js) which prints the authentication details of the user

📋 Deploy the app from the `node` folder using `webapp up`, using the existing App Service Plan, specifying Node 16 as the runtime.

<details>
  <summary>Not sure how?</summary>

List the runtimes to find Node.js:

```
az webapp list-runtimes --os Windows
```

And find the plan name:

```
az appservice plan list -g labs-appservice-static -o table
```

Navigate back to the node folder:

```
cd ../node 
```

Create a new web app - we need to use the Windows Node runtime as the App Service Plan is Windows:

```
az webapp up -g labs-appservice-static --runtime NODE:16LTS --os-type Windows --plan <app-service-plan> -n <unique-dns-name> 
```

</details><br/>

You need to get your `up` command correct to make sure you use the existing app plan - the defaults don't match the actual plan, so you'll get errors unless you explicitly provide the settings.

> When the deployment completes, check in the Portal and you'll see a new App Service, but using the same App Service Plan.

Explore each App Service in the Portal. Can you find the machine name which is hosting the app? Is it the same machine for both?

## Lab

Browse to the URL for your new Node.js app and you should see the statis HTML. Try the /user endpoint and you'll see the authentication details are `undefined`. 

The app needs an authentication provider configured. The code expects an Azure account; configure the app in the Portal to add an identity provider and confirm you need to log in when you browse to the app again.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

Delete the RG to clean up:

```
az group delete -y -n labs-appservice-static --no-wait
```
