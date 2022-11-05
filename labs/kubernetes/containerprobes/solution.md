# Lab Solution

Here's my solution:

- [deployment-productionized.yaml](solution/deployment-productionized.yaml) - adds (very aggressive) container probes

```
kubectl apply -f labs/kubernetes/containerprobes/solution
```

Open two terminals and you can watch the repair and scale in action:

```
kubectl get pods -l app=rngapi --watch

kubectl get endpoints rngapi-lb --watch
```

> Call the API again with curl. You'll still see failures if you go too fast, but leave it a few seconds between retries and the app comes back online.

If you keep calling then eventually all the Pods will go into CrashLoopBackOff because Kubernetes thinks the app is unstable.


> Back to the [exercises](README.md)