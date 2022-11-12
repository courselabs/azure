# Kubernetes Storage

Kubernetes has lots of abstractions to model your app, so you can describe it in a generic way that works on all clusters. For storage you can define different types of _volume_ which represent storage units, and mount them into your application Pods. 

The storage mounts appear as part of the container filesystem, but they're actually stored outside of the container - AKS uses standard Azure resources: disks and file shares. This lets you push configuration settings into the container as read-only files, and store application state outside of the container.

## Reference

- [Volumes in Kubernetes](https://kubernetes.io/docs/concepts/storage/volumes/)

- [Storage in AKS](https://learn.microsoft.com/en-us/azure/aks/concepts-storage)

- [PersistentVolumeClaim - Kubernetes API spec](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#persistentvolumeclaim-v1-core)


## Create an AKS cluster

Start with a new Resource Group for the lab, using your preferred region:

```
az group create -n labs-aks-persistentvolumes --tags courselabs=azure -l eastus
```

Now create a small cluster we can use for the lab:

```
az aks create -g labs-aks-persistentvolumes -n aks02 --node-count 1 --node-vm-size Standard_D2s_v5 --no-wait --location eastus
```

> The `no-wait` flag means the command returns and the cluster gets created in the background.

While that's creating we'll work with the local cluster. Be sure you have Docker Desktop running and your CLI is pointing to it:

```
kubectl config use-context docker-desktop

# check this is your local cluster:
kubectl get nodes
```

## Volumes and VolumeMounts

We'll be using a simple app which reads config files and writes to files in various locations. It's a .NET 6.0 background worker app - the main code is in [Worker.cs](/src/queue-worker/src/Worker.cs). The app is available on Docker Hub at [courselabs/queue-worker:6.0](https://hub.docker.com/r/courselabs/queue-worker/tags).

The first version we'll use mounts a ConfigMap to load config settings:

- [v1/configmap.yaml](./specs/v1/configmap.yaml) - contains a full appsettings.json file, with logging and application config
- [v1/deployment.yaml](./specs/v1/deployment.yaml) - models a Pod which loads the ConfigMap as a volume mount, taking the appsettings.json file and loading it into the /app directory

> Be sure you're clear what's defined in this model. The ConfigMap stores a JSON file. The Pod loads the ConfigMap as a volume, which makes it usable for the container, and the container actually loads the file as a volume mount.

📋 Run this app on your local cluster and print the Pod logs to check that the app is working.

<details>
  <summary>Not sure how?</summary>

All the specs are in the `v1` folder:

```
kubectl apply -f labs/aks-persistentvolumes/specs/v1
```

Find the Pod name:

```
kubectl get pods
```

Print the logs:

```
kubectl logs -f <pod-name>
```

</details><br/>

You should see from the application logs that it does some work and then sleeps for 20 seconds in a loop.

You can execute commands inside a running container with Kubernetes using the `exec` command. Kubernetes runs the command and prints the output it gets back.

Run this to show the contents of the files the app is writing inside the container:

```
kubectl exec -it deploy/queue-worker -- cat /mnt/cache/app.cache

kubectl exec -it deploy/queue-worker -- cat /mnt/database/app.db
```

> The app writes a line to each file every 20 seconds - the line includes the host name, which in Kubernetes is the Pod name

This is all looking good, so far.

## Container writeable storage

When you write data in containers, the storage has the same lifecycle as the container. When your Pod gets replaced you'll get a new container with a new filesystem, and any data written by the previous container will be lost. That will happen every time you update to use a new container image, or change some other part of the Pod spec.

📋 Delete your queue-worker Pod and wait for the Deployment to create a replacement Pod. When it's running, print out the contents of the `app.db` file in the new Pod.

<details>
  <summary>Not sure how?</summary>

The Deployment's job is to make sure there's one Pod for your app. If you delete the Pod, the Deployment will create a replacement:

```
kubectl delete pod <pod-name>
```

Watch Pods to see the replacement start up:

```
kubectl get pods --watch
```

When the new Pod is running, check the db file:

```
kubectl exec -it deploy/queue-worker -- cat /mnt/database/app.db
```

</details><br/>

You'll see that the database file has only got writes from the new Pod - the data file from the previous Pod was inside the container's filesystem, and that container has been deleted and replaced.

## EmptyDirs and PersistentVolumeClaims

A new version of the application model uses the same container image and ConfigMap, but it adds two writeable volumes to the Pod spec:

- [v2/deployment.yaml](./specs/v2/deployment.yaml) - mounts the cache directory to an EmptyDir volume and the database directory to a PersistentVolumeClaim volume

- [v2/pvc.yaml](./specs/v2/pvc.yaml) - models the PersistentVolumeClaim (PVC)

We covered these details of Kubernetes storage in the [PersistentVolumes lab](/labs/kubernetes/persistentvolumes/README.md). As a reminder - _EmptyDir_ is a piece of storage which has the lifecycle of the Pod, so if the Pod needs to restart the container then the data survives. _PersistentVolumeClaim_ is a request for the cluster to provide some storage the Pod can attach to - we don't specify any type of storage here, just the amount we need.

📋 Deploy the new version of the app on Docker Desktop. Let the Pod run for a while, then delete it. Check the database and cache files in the replacement Pod. Has the data been carried over from the previous Pod?

<details>
  <summary>Not sure how?</summary>

Deploy the v2 specs:

```
kubectl apply -f ./labs/aks-persistentvolumes/specs/v2
```

Check the data files:

```
kubectl exec -it deploy/queue-worker -- cat /mnt/cache/app.cache

kubectl exec -it deploy/queue-worker -- cat /mnt/database/app.db
```

Now delete the Pod:

```
kubectl delete pod <pod-name>
```

Watch Pods to see the replacement start up:

```
kubectl get pods --watch
```

When the new Pod is running, check the files again:

```
kubectl exec -it deploy/queue-worker -- cat /mnt/cache/app.cache

kubectl exec -it deploy/queue-worker -- cat /mnt/database/app.db
```

</details><br/>

You should see that the cache file is new - the EmptyDir volume in this Pod replaces the old one, so the data is lost. The database file is retained though, you'll see entries from the old Pod and the new one. It's stored in a PersistentVolume which has a separate lifecycle from any Pods, so the data is stored until the volume is deleted.

## PVCs and Storage Classes in AKS

The queue-worker model doesn't use any cluster-specific configuration, so it will work in the same way on your new AKS cluster.

Connect your Kubernetes CLI to the new AKS cluster:

```
az aks get-credentials -g labs-aks-persistentvolumes -n aks02 --overwrite
```

📋 Repeat the same steps on your AKS cluster - deploy the v2 app spec, let the Pod start and then delete it. Check the database file in the replacment Pod to make sure the data is persisted between Pods.

<details>
  <summary>Not sure how?</summary>

Deploy the v2 specs and wait for the Pod to start:

```
kubectl apply -f ./labs/aks-persistentvolumes/specs/v2

kubectl get pods --watch
```

When it's been running for a few seconds delete the Pod:

```
kubectl delete pod <pod-name>

kubectl get pods --watch
```

When the new Pod is running check the db file:

```
kubectl exec -it deploy/queue-worker -- cat /mnt/database/app.db
```

</details><br/>

You'll see both Pods writing to the same file. Where is the data actually stored? There are several options which you can choose between - Kubernetes calls them _Storage Classes_:

```
kubectl get storageclass
```

These are platform-specific. AKS offers Azure storage services, Docker Desktop just uses the disk on your machine. But they both have a default Storage Class for PVCs, which is why you can use the exact same application model.

## Lab

The default Storage Class in AKS uses a virtual disk, which has good I/O performance but can only be attached to one node at at a time. Sometimes you need shared storage, and the other option is to use the Storage Class called `azurefile`. That uses the Azure File Share service which can be accessed by many Pods on many nodes.

Write a new PVC which specifies the Azure Files Storage Class, and amend the Deployment spec so the database volume uses your new PVC, and scale up to 3 replicas. The app will behave in the same way, but if you explore in the Azure Portal you should be able to access the database file from there too.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources, including the storage.

```
az group delete -y --no-wait -n labs-aks-persistentvolumes
```

Now change your Kubernetes context back to your local Docker Desktop:

```
kubectl config use-context docker-desktop
```