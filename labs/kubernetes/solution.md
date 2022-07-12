# Lab Solution

Start by deleting the existing Pod and creating the Deployment:

```
kubectl delete pod simple-web

kubectl apply -f ./labs/kubernetes/specs/update/deployment.yaml
```

Check the Pods now and you'll see one with a random name; that was created by the Deployment:

```
kubectl get pods
```

Browse to http://localhost:8099/ and you'll see the environment is DEV again.

This sample solution adds the ConfigMap reference to the Pod template, loading the data as environment variables:

- [lab/deployment.yaml](./lab/deployment.yaml)

You can change the Deployment object by running `apply` again:

```
kubectl apply -f ./labs/kubernetes/lab/deployment.yaml
```

List the Pods again and you'll see a different random name - the Pod spec has changed, so the Deployment deletes the old Pod and creates a new one: 

```
kubectl get pods
```

Refresh the app and you'll see the config is loaded from the ConfigMap.