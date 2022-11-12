# Docker 101

How would you run a .NET app on Azure? You could provision a VM, then connect to it and install .NET, download your app binaries, set up the configuration and start the app. It's hard to automate all those steps, time-consuming to spin up a new instance and difficult to keep multiple instances in sync. Or you could use App Service, but there's still a lot to set up and you end up with a different hosting environment than you have locally.

Enter Docker - where you build all your application components and depdencies into a package called an _image_ and use that to run instances of your apps called _containers_.

## Reference

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) - the easiest way to run containers on your local machine
- [Getting Started guide](https://docs.docker.com/get-started/) from Docker
- [.NET container images](https://hub.docker.com/_/microsoft-dotnet) - .NET Core & 6 cross-platform images
- [.NET Framework container images](https://hub.docker.com/_/microsoft-dotnet-framework) - .NET 3.5 & 4.8 Windows images

## Run an .NET web container

We'll run containers locally first and later see our options for running them in Azure. Make sure you have Docker Desktop running - you'll see the Docker whale icon in your taskbar (if you're running on Windows and you've used Docker Desktop before, be sure you're in Linux container mode).

You use the `docker` command to run and manage containers. It has built-in help text, just like the `az` command:

```
docker --help
```

The command you'll use most to start with is `docker run` which starts a new container from an image. Run this command to start a simple web server in a container:

```
docker run -d -p 8081:80 nginx:alpine 
```

You'll see lots of output, ending with a long random string which is the unique ID of your new container. What has the command done?

- it runs a container from the `nginx:alpine` Docker image, which is publicly available and free to use. It has the Nginx web server installed and configured on an Alpine Linux OS base

- the `-d` flag puts the container in the background, so it carries on running when the command returns

- `-p` publishes a port on the container, so Docker can route network traffic into the container. For this container Docker will listen on port `8081` of your machine and send any traffic into port `80` of the container.


> Now browse to http://localhost:8081 and you'll see a response from your new web server.

You use other `docker` commands to manage your container apps:

```
# list running containers:
docker ps

# get the logs from a container:
docker logs <container-id>
```

📋 Run another container from Microsoft's **ASP.NET sample app** image - you can search for it on [Docker Hub](https://hub.docker.com). Run your ASP.NET container in the background and publish port `8082` from your machine to port `80` in the container.

<details>
  <summary>Not sure how?</summary>

Search for _.NET_ on Docker Hub and you'll find a page that lists all the images, including [ASP.NET](https://hub.docker.com/_/microsoft-dotnet-aspnet/). That shows you there's a sample app in the image called `mcr.microsoft.com/dotnet/samples:aspnetapp`.

```
docker run -d -p 8082:80 mcr.microsoft.com/dotnet/samples:aspnetapp
```

</details><br/>

> When your new container is running, browse to http://localhost:8082

Is your Nginx app still running? What version of .NET is inside the container? Can you print the logs from the ASP.NET sample app?

## Runtime & SDK images

Microsoft owns the Docker images for .NET and it publishes different variations - you've seen ASP.NET for web apps, there are also runtime images for console apps, and SDK images you can use to build applications.

You can run a container interactively, so you connect to a shell session in the container. This is like creating a VM in the cloud and running SSH to connect.

Run an interactive container from the base ASP.NET image and you can explore the environment:

```
docker run -it --entrypoint sh mcr.microsoft.com/dotnet/aspnet:6.0

dotnet --list-runtimes

dotnet --list-sdks

exit
```

You'll see the .NET and ASP.NET runtimes are installed, but there are no SDKs. You can use this image to run compiled apps, but not to build apps from source code.

📋 Run an interactive container from the .NET 6.0 **SDK** image, which you can find on Docker Hub. Use it to create and run a new console application.

<details>
  <summary>Not sure how?</summary>

There's a separate image for the [.NET SDK](https://hub.docker.com/_/microsoft-dotnet-sdk/):

```
docker run -it --entrypoint sh mcr.microsoft.com/dotnet/sdk:6.0
```

That gives you a shell session inside the container, which has the .NET 6.0 runtime and SDK installed.

Now you can create and run an app:

```
dotnet new console -o labs-docker

cd labs-docker

dotnet run
```

</details><br/>

When you run the new app you'lll see the standard _Hello, World!_ output. If this reminds you of the Azure Cloud Shell experience, those shell sessions actually run in containers behind the scenes.

Run `exit` to leave the interactive container and come back to your terminal session.

## Build .NET apps in containers

Building apps inside a container is a good way of experimenting, but the real value of Docker is in packaging your own Docker images:

- this [Dockerfile](/src/simple-web/Dockerfile) is a script which packages an ASP.NET app in Docker. It uses the SDK image to build the app and the ASP.NET image to run the app.

There's a lot more you can do with Dockerfiles, but this is a good start. You can build and run a .NET 6.0 app without installing .NET 6.0 on your machine.

Run this to build an image called `simple-web` from the Dockerfile and the source code:

```
docker build -t simple-web src/simple-web
```

You'll see Docker print the output from `dotnet` commands, building and compiling the app..

📋 Run a background container from the new image and publish port `8083` from your machine to port `80` in the container.

<details>
  <summary>Not sure how?</summary>

It's the same `docker run` command.

The image name can be a reference to Docker Hub or Microsoft's container registry, or to a local image:

```
docker run -d -p 8083:80 simple-web 
```

</details><br/>

> Browse to http://localhost:8083 to see the app

The app is very simple, but you can improve it. Edit code in the `src/simple-web/src` folder and run the build command again to package up your changes. Test it by running a new container - you can't repeat the same `docker run` command though - why is that?

## Lab

Container images are static packages - they're really just ZIP files with all your application binaries and dependencies, the runtime and operating system tools. Image names often include a version number, and you can publish different images for different versions of your app. Wherever you run a container from the image, the app will always behave in the same way, because the starting point is always the same.

Typically things change between environments, so you need a way to inject configuration settings into the app when you run a container. The simplest way to do that is with _environment variables_, which you can set when you run the container and get read by the .NET configuration system. The simple web app uses a config setting to show the environment name - run a new container listening on port `8084` which shows the environment name `PROD` on the homepage.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You'll have lots of containers running, but containers are intended to be disposable.

Run this command to remove them all:

```
docker rm -f $(docker ps -aq)
```
