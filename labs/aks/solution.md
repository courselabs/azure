# Lab Solution

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

Browse back to your public IP address - the Service IP address hasn't changed. Refresh without using the browser cache (e.g. Ctrl-F5 on Windows) and you'll see response coming from different Pods. The Pods all have the `app=simple-web` label, and the Service load-balances requests between them.