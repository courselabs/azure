# Lab Hints

You can run Windows containers on ACI using the `az` command but the Docker CLI plugin only supports Linux containers.

Container images are built for a specific OS and CPU architecture. `courselabs/simple-web:6.0` is a multi-architecture image, so if you run it on Linux you'll get the Linux version, on Windows you'll get the Windows version. 

With the Azure CLI you can set the OS to use when you create a container and ACI will pick the right image. Or you can check Docker Hub to find the specific Windows image tag as well as setting the OS.

Azure doesn't support ARM processors (yet), so if you try to run a container using the Linux ARM64 image, expect to see an error.

> Need more? Here's the [solution](solution.md).