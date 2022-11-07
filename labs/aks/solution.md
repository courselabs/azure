# Lab Solution

## Multiple replicas

The field is called `replicas` and you set it as part of the Deployment spec (not part of the Pod spec in the template):

```
apiVersion: apps/v1
kind: Deployment
metadata:
  name: simple-web
spec:
  replicas: 2
  selector:
    # ...
  template:
    # ...
```

The sample file in [labs/aks/lab/deployment.yaml](./lab/deployment.yaml) sets the replica count to 4.

You can use that file to update the Deployment, then if you watch the Pods you'll see three more get created:

```
kubectl apply -f labs/aks/lab/deployment.yaml

kubectl get pods --watch
```

When all the Pods are running, you can print the list with the `-o wide` flag to see which node the Pods use:

```
kubectl get pods -o wide
```

You should see Pods spread across your two nodes.

Browse back to your public IP address - the Service IP address hasn't changed. Refresh without using the browser cache (e.g. Ctrl-F5 on Windows) and you'll see response coming from different Pods. If no try curl:

```
curl <service-ip-address>
```

The Pods all have the `app=simple-web` label, and the Service load-balances requests between them.

## Config Changes

The ConfigMap is defined in YAML so you can update the value and redeploy it - the sample in [lab/configmap.yaml](./lab/configmap.yaml) sets the environment name to "UAT":

```
kubectl apply -f labs/aks/lab/configmap.yaml
```

Check the Pods and you'll see they don't restart. Try the site and you'll still see the old value. The config setting is loaded as an environment variable, and that can't be changed for the life of a Pod. Updating the ConfigMap doesn't trigger a rollout for Deployments which use the data - you need to do that manually:

```
kubectl rollout restart deploy/simple-web

kubectl get pods --watch
```

> The Pods get replaced and the new Pods will load the new config setting - try the app again and you'll see the updated value.