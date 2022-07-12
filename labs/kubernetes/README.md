# Kubernetes 101

Kubernetes is a container platform. It runs applications using the same container images you use with Docker and ACI, but it works declaratively instead of imperatively. You don't tell Kubernetes to run a container with a command, instead you describe what your application needs to have in a YAML model and send that to Kubernetes. It decides what needs to happen to get your application up by comparing the model with what's currently running. There's a lot to learn in the Kubernetes model, but it has the huge benefit of being portable - you can run your app with Kubernetes on Docker Desktop or on a managed service in Azure using the same model.

## Reference

- [Pod docs](https://kubernetes.io/docs/concepts/workloads/pods/) and [Pod API spec](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#pod-v1-core)

- [Services](https://kubernetes.io/docs/concepts/services-networking/service/) and [Service API spec](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#service-v1-core)

- [ConfigMaps](https://kubernetes.io/docs/concepts/configuration/configmap/) and [ConfigMap API spec](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#configmap-v1-core)

- [Pod controllers](https://kubernetes.io/docs/concepts/architecture/controller/) and [Deployment API spec](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#deployment-v1-apps)

## Nodes

Make sure you have Docker Desktop configured to run Kubernetes (see the [setup guide](/setup/README.md)). A Kubernetes environment is called a _cluster_ and a cluster has one or more machines which can run containers. The machines in a cluster are called _nodes_. Docker Desktop gives you a single-node cluster running on your machine, and it also sets up the Kubernetes command line `kubectl`.

Kubectl has integrated help like the Docker and Azure CLIs, but it uses a different syntax where the action comes before the type of object.

Run this to list all the nodes in your cluster - you should see a single node which is your machine running Docker Desktop:

```
kubectl get nodes
```

The `get` command returns a list of resources. The default output is human-readable, but you can change the format like you can with `az` which helps with automation. The next level of detail comes from the `describe` command, which you run with a resource type and name:

```
kubectl describe node <your-node-name>
```

> You'll see a lot of detail about the node, including the OS and CPU architecture. Kubernetes is a cross-platform system, a cluster can be composed of multiple nodes running different OS and CPU architectures, so a single cluster can run Windows and Linux containers.

## Pods

Kubernetes wraps containers in resources called _Pods_. A Pod is assigned to a single node and its job is to keep the container running - it can replace failed containers so you get more resilience than running containers directly with Docker.

- [labs/kubernetes/specs/pod.yaml](./specs/pod.yaml) - describes a very basic Pod to run the simple-web application in a container, adding a label which we'll use later

You use the `kubectl apply` command to send your YAML model to the cluster. Then Kubernetes looks at what's running in the cluster, compares it to the model and takes actions to bring your application up to the desired state.

Create the application Pod:

```
kubectl apply -f labs/kubernetes/specs/pod.yaml
```

> You'll see output telling you the Pod has been created.

Repeat the command and you'll see how the desired-state approach works.

📋 List all your Pods with Kubectl and then print the details of the Pod we just created.

<details>
  <summary>Not sure how?</summary>

It's the same `get` command to list resources, you just add the resource type:

```
kubectl get pods
```

And the same `describe` command with the resource type and name:

```
kubectl describe pod simple-web
```

</details><br/>

Kubernetes manages containers for you, using Docker or another container runtime. That means you don't typically work with containers directly, you always use Kubernetes to get to the underlying container. There are Kubectl commands for most container management tasks.

📋 Print the logs from the application container in the simple web Pod.

<details>
  <summary>Not sure how?</summary>

Just running `kubectl` will show all the available commands. Their you'll see the `logs` command:

```
kubectl logs simple-web
```
</details><br/>

The output is the same as we've seen before because it's the same container image running the same application.

We can't browse to the app yet, because Pods aren't accessible on the network. To send traffic into a Pod you need another object which acts as a router.

## Services

_Services_ are the Kubernetes network abstraction. A Service listens for incoming traffic (from inside or outside the cluster, depending on the type of Service) and forwards it on to a Pod.

Services are loosely-coupled to Pods using labels. A Service is defined with a label selector which can match zero or many Pods. Matched Pods are potential targets for routing incoming traffic. We added a label to the simple web Pod:

```
kubectl get pods --show-labels
```

And we can use that `app` label to find Pods for a Service:

- [labs/kubernetes/specs/service.yaml](./specs/service.yaml) - defines a LoadBalancer Service which listens for traffic coming into the cluster on port 8080, and routes it into a Pod with the `app=simple-web` label on port 80.

📋 Create the Service in your cluster from the YAML file, and then list the Service objects.

<details>
  <summary>Not sure how?</summary>

The `apply` command works for all resource types:

```
kubectl apply -f labs/kubernetes/specs/service.yaml
```

Then use the `get` command:

```
kubectl get services
```

</details><br/>

The printed output for a Service includes an external IP address, which is the address you use to reach your LoadBalancer Service from outside the cluster. With Docker Desktop the address will be `localhost`, so you can browse to the simple .NET app running in Kubernetes:

> http://localhost:8099

There's a lot more you can do with Kubernetes, but even this simple app shows you how powerful the desired-state model is - you can use the same app description on any Kubernetes platform.

## ConfigMaps

There's one more resource we'll walk through - the _ConfigMap_ which you can use to isolate application configuration from the Docker image and the Pod spec. ConfigMaps are static pieces of data which get stored in the cluster. They can contain complex files like `appsettings.json` or `web.config`, or they can store simple key-value pairs.

- [labs/kubernetes/specs/configmap.yaml](./specs/configmap.yaml) - defines a ConfigMap we can use with the web app to set the application environment.

📋 Create the ConfigMap object and then describe it with Kubectl to print all the details.

<details>
  <summary>Not sure how?</summary>

It's the same `apply` command:

```
kubectl apply -f labs/kubernetes/specs/configmap.yaml
```

Then use the `describe` command:

```
kubectl describe configmap simple-web-config
```

</details><br/>

When you print the details of a ConfigMap you can see all the data. They're not suitable for sensitive data like API keys and credentials, but they're perfect for application settings which change between environments.

ConfigMaps aren't linked to Pods - a ConfigMap can be used by many Pods, or no Pods at all. This ConfigMap isn't being used yet, but we can update the application Pod to read configuration settings from the ConfigMap:

- [labs/kubernetes/specs/update/pod.yaml](./specs/update/pod.yaml) - changes the Pod spec to load the ConfigMap data as environment variables. Remember the application is set up to read config settings from environment variables.

📋 Try to deploy the changed Pod spec and you'll get an error - some Pod settings are fixed, including the environment variables. So you'll need to delete the Pod and then recreate it with the new spec.

<details>
  <summary>Not sure how?</summary>

This will fail because environment variables can't be changed in an existing Pod:

```
kubectl apply -f ./labs/kubernetes/specs/update/pod.yaml
```

So delete the Pod first:

```
kubectl delete pod simple-web
```

And then create the updated version:

```
kubectl apply -f ./labs/kubernetes/specs/update/pod.yaml
```

</details><br/>

> Refresh your browser at http://localhost:8099; do you see how the ConfigMap contents are filtered into the app settings?

## Lab

You may have been surprised that you can't update the spec of a running Pod - that seems like a pretty severe limitation. It's because the Pod is a virtual environment with some features that are set on creation and can't be changed, like environment variables, because they can't be changed in the underlying container. 

It's not an issue in a real application though because you don't often use Pods directly. Instead you use a Pod _controller_ resource which includes a Pod template. You can update the Pod template in a controller and it will make the change for you by removing old Pods and replacing them with new ones. This is a  Deployment spec for the app - you can see it contains the Pod spec as a template:

- [labs/kubernetes/specs/update/deployment.yaml](./specs/update/deployment.yaml) - the Deployment object uses a label selector to find the Pods it manages

Delete your application Pod and create the Deployment. Verify the app is still available at http://localhost:8099 and its using the default environment name. Then change your Deployment to load the ConfigMap into the Pod environment variables and redeploy. Do you need to delete anything this time?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete multiple Kubernetes objects in one command:

```
kubectl delete deployment,service,configmap --all
```

When a Deployment is deleted, it removes all the Pods it manages.