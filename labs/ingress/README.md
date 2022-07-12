# Ingress and Application Gateway

You can route network traffic into a Kubernetes cluster with a LoadBalancer Service. In AKS that gives you a public IP address, but when you start using Kubernetes you tend to run lots of apps on a single cluster and you don't want lots of random IP addresses. Instead you want to use a single IP address and route incoming traffic by the HTTP domain name, so a single cluster can serve `myapp.com`, `api.myapp.com` and `otherapp.co.uk` all from one public IP address that you set in your DNS service. Kubernetes supports that with _Ingress_ objects, which integrate nicely with the Azure Application Gateway service.

## Reference

- [Azure Application Gateway docs](https://docs.microsoft.com/en-gb/azure/application-gateway/)

- [Ingress in Kubernetes](https://kubernetes.io/docs/concepts/services-networking/ingress/) and the [Ingress API spec](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#ingress-v1-networking-k8s-io)

- [AGIC - Application Gateway Ingress Controller](https://docs.microsoft.com/en-us/azure/application-gateway/ingress-controller-overview)

## AKS add-ons

Kubernetes uses an ingress controller to manage traffic routing. The ingress controller is just an app which runs in Pods so you can deploy it manually using Kubectl, but with AKS you can deploy the Application Gateway Ingress Controller as an add-on. 

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-ingress --tags courselabs=azure -l eastus
```

You can create an AKS cluster with the AGIC add-on and it will create everything for you. You need a name for your new Application Gateway, and a subnet range of IP addresses to use:

```
az aks create -g labs-ingress -n aks03 --node-count 2 --enable-addons ingress-appgw --appgw-name appgw --appgw-subnet-cidr "10.10.0.0/16" --location eastus
```

> It's better to create the Application Gateway first. Then you can configure the Public IP address and the Gateway how you want them.

That will take a while to run, so we'll work with Ingress objects on our local cluster while it's going.

## Deploy a local Ingress Controller

Ingress controllers are optional in most Kubernetes environments - Docker Desktop doesn't install one by default, but you can enable ingress by deploying this:

```
kubectl apply -f labs/ingress/specs/local-ingress-controller/
```

That will create a lot of resources we haven't covered, but you can treat your ingress controller as a black box for now. It's job is to watch out for Ingress objects and set up the routing rules, linking domain names to your application Services.

Now we need a web application we can use:

- [labs/ingress/specs/simple-web/deployment.yaml](./specs/simple-web/deployment.yaml) - models the simple-web app, specifying the environment name config setting inside the Pod spec
- [labs/ingress/specs/simple-web/service.yaml](./specs/simple-web/service.yaml) - makes the Pod accessible to other Pods on the network

> This uses a Service type of `ClusterIP`, which means the Service is only accessible within the cluster. The Ingress rule will reference this Service as the target, and the Ingress controller will route traffic to the Pod via this Service.


📋 Deploy the simple web app to your cluster, then print your Service list.

<details>
  <summary>Not sure how?</summary>

The YAML files are all in the same folder:

```
kubectl apply -f labs/ingress/specs/simple-web/
```

Run this to show your Services:

```
kubectl get services
```

</details><br/>

The `simple-web` Service has a cluster IP address, which can be reached by any Pod in the cluster, running on any node. There's no external IP address though, because this is just an internal Service.

## Create the Ingress rule

Ingress objects are fairly straightforward:

- [labs/ingress/specs/ingress-local.yaml](./specs/ingress-local.yaml) - routes traffic for the domain `simple-web.local` into the `simple-web` Service, using a named port

📋 Create the Ingress object and print out its details

<details>
  <summary>Not sure how?</summary>

It's the same `apply` command for all resources:

```
kubectl apply -f labs/ingress/specs/ingress-local.yaml
```

And you can print the details of the object with:

```
kubectl get ingress

kubectl describe ingress simple-web
```

</details><br/>

The details of the Ingress object show you the IP address of the target Service, which will match what you see from the Service details.

Now you can browse to http://simple-web.local - but you won't see anything because your machine doesn't recognise that address. You can add an entry in your [hosts file](https://en.wikipedia.org/wiki/Hosts_(file)) - this script will do it for you:

```
# using Powershell - your terminal needs to be running as Admin:
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force
./scripts/add-to-hosts.ps1 simple-web.local 127.0.0.1

# on macOS or Linux - you'll be asked for your sudo password:
sudo chmod +x ./scripts/add-to-hosts.sh
./scripts/add-to-hosts.sh simple-web.local 127.0.0.1
```

> Now you can browse to http://simple-web.local:8008 to see the app

The Ingress controller is listening on port 8008 - it receives the request from your browser and uses the domain name to find the matching Ingress rule. Ingress controllers are really just reverse proxies which fetch responses from internal services and send them back to the client.

## Deploy with Application Gateway on AKS

Your cluster should be ready now, and we can deploy the same app using a real public domain name.

📋 Download the connection credentials for Kubectl to use your AKS cluster.

<details>
  <summary>Not sure how?</summary>

This command creates the Kubectl context and sets it as the current one:

```
az aks get-credentials -g labs-ingress -n aks03
```

It's always a good idea to list the nodes, to check you're using the right cluster:

```
kubectl get nodes
```

</details><br/>

You can deploy the app with the same model you used with Docker Desktop:

```
kubectl apply -f labs/ingress/specs/simple-web/
```

But you'll need a different Ingress object. We want to use a DNS name that routes to the public IP address for the Application Gateway.

Find the IP address object that AKS created:

```
az network public-ip list -o table
```

> It should be named `appgw-appgwpip` and it will be in the AKS-generated `MC_` Resource Group

You can add a DNS name to the IP address with an update command. You'll need to think of a unique domain name:

```
az network public-ip update -n appgw-appgwpip -g <aks-mc-rg-namge> --dns-name <dns-name>

# e.g. on my cluster I ran:
# az network public-ip update -n appgw-appgwpip -g MC_labs-ingress_aks03_eastus --dns-name labs-ingress-01
```

The response shows the `fqdn` field which will be the domain name suffix for your Ingress rule.

Now you'll need to edit [labs/ingress/specs/ingress-aks](./specs/ingress-aks.yaml) and replace `[PLACEHOLDER]` with your FQDN.

📋 Create the Ingress object and print the details. Browse to the domain name - do you see the app?

<details>
  <summary>Not sure how?</summary>


Make sure you've set your own DNS name in the YAML and then apply it:

```
kubectl apply -f labs/ingress/specs/ingress-aks.yaml
```

Your Ingress object should show the DNS name and the public IP address:

```
kubectl get ingress
```

</details><br/>

Browse to the host name and you should see the application, routed via Application Gateway.

## Lab

It's very useful to understand how the Application Gateway actually routes traffic into your containers. Scale up your Deployment to run 4 Pods and find the internal IP addresses of your Pods. Then navigate through the Application Gateway setup in the Portal. Can you see how the routing works, and how there are healthchecks to make sure traffic is only routed to healthy Pods.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources, including the storage.

```
az group delete -y -n labs-ingress
```

Now change your Kubernetes context back to your local Docker Desktop:

```
kubectl config use-context docker-desktop
```