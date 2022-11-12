# Azure Kubernetes Service

Kubernetes is an open-source platform, but lots of vendors supply their own packaged versions. Azure Kubernetes Service (AKS) is a managed Kubernetes service - you create an AKS cluster and deploy your apps using the Kubernetes model. Azure takes care of provisioning VMs for the cluster nodes and installing and configuring Kubernetes. It also simplifies tasks like scaling the cluster to add or remove nodes, upgrading the Kubernetes version and integrating with other Azure services.

## Reference

- [Kubernetes Service documentation](https://docs.microsoft.com/en-gb/azure/aks/)

- [`az aks` commands](https://docs.microsoft.com/en-us/cli/azure/aks?view=azure-cli-latest)

- [Deployment - Kubernetes API spec](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#deployment-v1-apps)

## Explore in portal

Open the Portal and search to create a new Kubernetes Service resource. There's a lot you can configure for AKS:

- the number of nodes and the VM size
- the _Presets_ give you some good starting configurations
- AKS has _Node Pools_ which are groups of nodes that share the same setup - you may have 10 Linux nodes in one pool, 5 Linux servers with GPU in another pool and 2 Windows servers in a third pool, all in the same cluster
- clusters can be secured with standard Kubernetes Role-Based Access Control (RBAC) linked to Azure accounts
- AKS can be integrated with ACR so you can run containers from private ACR images without extra configuration

Production-grade deployments of AKS get complicated, but we'll start with a simple one with the CLI.

## Create an AKS cluster with the CLI

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-aks --tags courselabs=azure -l eastus
```

You use the `az aks create` command to create a new cluster.

📋 Create a cluster called `aks01` with two nodes using the `Standard_D2s_v5` VM size (or a size that's valid in your chosen region).

<details>
  <summary>Not sure how?</summary>

Run `az aks create --help` and you'll see there are lots of options. Most are optional, this will create the setup:

```
az aks create -g labs-aks -n aks01 --node-count 2 --node-vm-size Standard_D2s_v5 --location eastus
```

</details><br/>

It will take a while for the cluster to be created. While the CLI is running, browse back to the Portal and look at your Resource Groups. You'll see `labs-aks` with your AKS cluster in it, but you'll also see another RG with a name that begins `MC_`. Have a look in there - what do you think that RG is?

## Using the cluster

AKS brings together a lot of other Azure resources and takes care of managing them all for you. Those resources are kept in a separate RG which you shouldn't work with - treat it as a black box which you manage through the AKS resource.

As soon as your cluster is ready, you can deploy apps to it. You use the same YAML models and the same Kubectl command line with AKS that you use with Docker Desktop and any other Kubernetes cluster. Kubectl has the idea of _contexts_ like the Docker CLI. You can add a context for your AKS cluster using the Azure command line.

📋 Download your cluster credentials with an `az aks` command.

<details>
  <summary>Not sure how?</summary>

List out the AKS commands:

```
az aks --help
```

You'll see `get-credentials` which downloads the access details you need to use Kubectl with your AKS cluster:

```
az aks get-credentials -g labs-aks -n aks01 --overwrite-existing 
```

</details><br/>

The Azure command line takes care of the details, but you can use Kubectl to see which clusters you can connect to:

```
kubectl config get-contexts
```

You'll see an asterisk next to your AKS cluster, meaning that's your current context. Now when you run Kubectl commands you're talking to your Kubernetes cluster in Azure:

```
kubectl get nodes
```

## Deploying applications

You can deploy the exact same Kubernetes application models in AKS that we've used in Docker Desktop. The YAML specs in the `labs/aks/specs` folder define an app that runs on any Kubernetes cluster:

- [configmap.yaml](./specs/configmap.yaml) - sets the environment name to be PROD
- [deployment.yaml](./specs/deployment.yaml) - is identical to the [Kubernetes lab](/labs/kubernetes/README.md)
- [service.yaml](./specs/service.yaml) - routes external traffic coming in to port 80 to the application Pod

📋 Run the app on your AKS cluster - you can deploy all the YAML files in one folder with a single command - then check the Pods and Services.

<details>
  <summary>Not sure how?</summary>

It's the same `kubectl apply` command - the path can be a single file, a folder, or a web address:

```
kubectl apply -f labs/aks/specs
```

Then list the resources:

```
kubectl get pods,services
```

</details><br/>

> The Pod should go into the _Running state_ pretty quickly, and the Service should have an external IP address.

If the external IP address says `<pending>` then you can run this command to watch the resource for updates:

```
kubectl get service simple-web --watch
```

As soon as the IP address is set, you can exit the watch with Ctrl-C/Cmd-C.

The IP address is your application's public IP address. Browse to it and you'll see the app. Look in the Portal to see if you can find the resources that provide the IP address and route traffic to the cluster VMs.

## Lab

We have spare capacity in the AKS cluster, so we can run more Pods to serve more users. Investigate how you can change the Deployment spec to run 4 Pods for the web application. You'll need to edit the YAML and apply the changes. When you have multiple Pods running, what happens if you repeatedly refresh the website in your browser? If you change the environment name from `PROD` in the configuration and redeploy, does the site update straight away?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources. When the AKS cluster gets deleted, it removes the managed `MC_` RG too:

```
az group delete -y --no-wait -n labs-aks
```

Now change your Kubernetes context back to your local Docker Desktop:

```
kubectl config use-context docker-desktop
```