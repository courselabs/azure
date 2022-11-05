# Ingress and Application Gateway

You can route network traffic into a Kubernetes cluster with a LoadBalancer Service. In AKS that gives you a public IP address, but when you start using Kubernetes you tend to run lots of apps on a single cluster and you don't want lots of random IP addresses. 

Instead you want to use a single IP address and route incoming traffic by the HTTP domain name, so a single cluster can serve `myapp.com`, `api.myapp.com` and `otherapp.co.uk` all from one public IP address that you set in your DNS service. Kubernetes supports that with _Ingress_ objects, which integrate nicely with the Azure Application Gateway service.

## Reference

- [Azure Application Gateway docs](https://docs.microsoft.com/en-gb/azure/application-gateway/)

- [Ingress in Kubernetes](https://kubernetes.io/docs/concepts/services-networking/ingress/) and the [Ingress API spec](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#ingress-v1-networking-k8s-io)

- [AGIC - Application Gateway Ingress Controller](https://docs.microsoft.com/en-us/azure/application-gateway/ingress-controller-overview)

## Create an Application Gateway

Open the Portal, navigate to create a new resource and search for 'application gateway' - click create. This is an unusual resource - each page has to be completed before you can move on. If you work through the pages you'll see:

- _Basics_ - with a fixed scale or autoscaling option, you can choose a tier and a VNet is required
- _Frontends_ - the IP address routing to the AppGW, usually a PIP
- _Backends_ - backend pools where traffic will be routed, this is the same concept as an Azure Load Balancer
- _Configuration_ - rotuing rules for incoming requests to be matched to a backend

You can set up an AppGW manually, specifying all the routing rules (e.g. that requests for mydomain.com get routed to a particular VMSS). With AKS that's all done automatically because the AppGW acts as an _ingress controller_ (a concept we covered in the [Ingress lab](/labs/kubernetes/ingress/README.md)).


Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-aks-ingress --tags courselabs=azure -l eastus
```

You can create an AKS cluster with the AGIC add-on and it will create everything for you - but it's better to create the Application Gateway first so you can set it up how you want to, and keep your AppGW running if you remove your AKS cluster.

The App Gateway will be the entrypoint to all your apps, so you need a public IP address. It also needs to be deployed into the same VNet you'll use for your AKS cluster.

📋 Create a PIP, VNet and two subnets to use for the deployment.

<details>
  <summary>Not sure how?</summary>

```
# create the PIP:
az network public-ip create -g labs-aks-ingress -n appgw-pip --sku Standard -l eastus --dns-name <unique-dns-name>

# the vnet:
az network vnet create -g labs-aks-ingress -n vnet --address-prefix 10.2.0.0/16 -l eastus

# and subnets:
az network vnet subnet create -g labs-aks-ingress --vnet-name vnet -n aks --address-prefixes 10.2.8.0/21

az network vnet subnet create -g labs-aks-ingress --vnet-name vnet -n appgw --address-prefixes 10.2.3.0/24
```

</details><br/>

Now create the application gateway - all the networking components need to be in the same region or you'll get an error:

```
# we need a v2 SKU to work with AKS:
az network application-gateway create -g labs-aks-ingress -n appgw  --public-ip-address appgw-pip --vnet-name vnet --subnet appgw --capacity 1 --sku Standard_v2 --priority "1" -l eastus
```

This will take a while to create. Check progress in the Portal, but while it's creating we can move on to creating the AKS cluster.

## AKS add-ons

AKS has a concept of [add-ons](https://learn.microsoft.com/en-us/azure/aks/integrations) which you can use to add new functionality to an existing cluster. We'll create the cluster now, and when both the cluster and the AppGW are ready we can integrate them with an add-on.

📋 Create an AKS cluster using the `azure` network plugin, and the other subnet in your vnet (you'll need the subnet ID).

<details>
  <summary>Not sure how?</summary>

Get the subnet ID:

```
az network vnet subnet show  -g labs-aks-ingress -n aks --vnet-name vnet --query id -o tsv
```

Create the cluster:

```
az aks create -g labs-aks-ingress -n aks04 --network-plugin azure --vnet-subnet-id '<subnet-id>' -l eastus
```

</details><br/>

> When you integrate AKS with a VNet the cluster needs permission to manage the network.

You'll see a message _Waiting for AAD role to propagate_ with its own progress bar. Your account needs elevated permissions in the subscription to create the role. That's fine on your own sub but in a corporate subscription that might be restricted.

When the AppGW is created, check it out in the Portal. The UX is very similar to the Load Balancer resource - AppGW is an enhanced type of load balancer - with a few extra features. Web Application Firewall (WAF) is a feature you definitely want to look at for production deployments.

When your AKS cluster is also ready you can deploy the AppGW add-on:

```
# get the AppGW ID:
az network application-gateway show -n appgw -g labs-aks-ingress --query id -o tsv

# enable the add-on:
az aks enable-addons -n aks04 -g labs-aks-ingress -a ingress-appgw --appgw-id '<appgw-id>' 
```

This setup all takes a while to run - but this is a production-grade deployment which is all ready for you to run scalable, reliable, public-facing apps with Kubernetes.

## Deploy with Application Gateway on AKS

Now everything is ready, we'll deploy a simple app which we can reach using a public URL.

📋 Download the connection credentials for Kubectl to use your AKS cluster.

<details>
  <summary>Not sure how?</summary>

This command creates the Kubectl context and sets it as the current one:

```
az aks get-credentials -g labs-aks-ingress -n aks04
```

It's always a good idea to list the nodes, to check you're using the right cluster:

```
kubectl get nodes
```

</details><br/>

We'll deploy the whoami application using a similar spec we used locally on Docker Desktop: [whoami.yaml](/labs/aks-ingress/specs/whoami.yaml). This creates the internal ClusterIP Service and the Deployment (this time with 10 replicas).

```
kubectl apply -f labs/aks-ingress/specs/whoami.yaml
```

For the Ingress object we need to set the DNS name to match the PIP. Run this to print the fully-qualified DNS name (FQDN) of your PIP:

```
az network public-ip show -g labs-aks-ingress -n appgw-pip --query 'dnsSettings.fqdn' -o tsv
```
> **Edit the file** [ingress-aks.yaml](labs/aks-ingress/specs/ingress-aks.yaml), replacing the placeholder <pip-fqdn> with your actual FQDN.

📋 Create the Ingress object from `labs/aks-ingress/specs/ingress-aks.yaml` and print the details. Browse to the domain name - do you see the app?

<details>
  <summary>Not sure how?</summary>

Make sure you've set your own DNS name in the YAML and then apply it:

```
kubectl apply -f labs/aks-ingress/specs/ingress-aks.yaml
```

Your Ingress object should show the DNS name and the public IP address:

```
kubectl get ingress
```

</details><br/>

Browse to the host name and you should see the application, routed via Application Gateway. Refresh and the load-balancing should work nicely between all the Pods.

## Lab

It's very useful to understand how the Application Gateway actually routes traffic into your containers. Navigate through the Application Gateway setup in the Portal. Can you see how the routing works, and how there are healthchecks to make sure traffic is only routed to healthy Pods?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources, including the storage.

```
az group delete -y --no-wait -n labs-aks-ingress
```

Now change your Kubernetes context back to your local Docker Desktop:

```
kubectl config use-context docker-desktop
```