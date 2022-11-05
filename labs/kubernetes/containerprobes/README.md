# Container Probes

It's usually straightforward to model your apps in Kubernetes and get them running, but there's more work to do before you get to production.

One of the main features Kubernetes has is that it can fix apps which have temporary failures, constantly testing components and taking action if they don't respond as expected. For that to work you need to tell Kubernetes how to test your apps and you do that with _container probes_.

## API specs

- [ContainerProbe](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#probe-v1-core)

<details>
  <summary>YAML overview</summary>

Container probes are part of the container spec inside the Pod spec:

```
spec:
  containers:
    - # normal container spec
      readinessProbe:
        httpGet:
          path: /health
          port: 80
        periodSeconds: 5
```

- `readinessProbe` - there are different types of probe, this one checks the app is ready to receive network requests
- `httpGet` - details for the HTTP call Kubernetes makes to test the app - non-OK response codes means the app is not ready
- `periodSeconds` - how often to run the probe

</details><br/>

## Self-healing apps with readiness probes

We know Kubernetes restarts Pods when the container exits, but the app inside the container could be running but not responding - like a web app returning `503` - and Kubernetes won't know.

The whoami app has a nice feature we can use to trigger a failure like that. 

📋 Start by deploying the app from `labs/kubernetes/containerprobes/specs/whoami`.

<details>
  <summary>Not sure how?</summary>

```
kubectl apply -f labs/kubernetes/containerprobes/specs/whoami
```

</details><br/>

You now have two whoami Pods - make a POST command and one of them will switch to a failed state:

```
# if you're on Windows, run this to use the correct curl:
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force; . ./scripts/windows-tools.ps1

curl http://localhost:8010

curl --data '503' http://localhost:8010/health

curl -i http://localhost:8010
```

> Repeat the last curl command and you'll get some OK responses and some 503s - the Pod with the broken app doesn't fix itself.

You can tell Kubernetes how to test your app is healthy with [container probes](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/). You define the action for the probe, and Kubernetes runs it repeatedly to make sure the app is healthy:

- [whoami/update/deployment-with-readiness.yaml](specs/whoami/update/deployment-with-readiness.yaml) - adds a readiness probe, which makes an HTTP call to the /health endpoint of the app every 5 seconds

📋 Deploy the update in `labs/kubernetes/containerprobes/specs/whoami/update` and wait for the Pods with label `update=readiness` to be ready.

<details>
  <summary>Not sure how?</summary>

```
kubectl apply -f labs/kubernetes/containerprobes/specs/whoami/update

kubectl wait --for=condition=Ready pod -l app=whoami,update=readiness
```

</details><br/>

> Describe a Pod and you'll see the readiness check listed in the output

These are new Pods so the app is healthy in both; trip one Pod into the unhealthy state and you'll see the status change:

```
curl --data '503' http://localhost:8010/health

kubectl get po -l app=whoami --watch
```

> One Pod changes in the Ready column - now 0/1 containers are ready.

If a readiness check fails, the Pod is removed from the Service and it won't receive any traffic.

📋 Confirm the Service has only one Pod IP and test the app.

<details>
  <summary>Not sure how?</summary>

```
# Ctrl-C to exit the watch

kubectl get endpoints whoami-np

curl http://localhost:8010
```

</details><br/>

> Only the healthy Pod is in enlisted in the Service, so you will always get an OK response.

If this was a real app the `503` could be happening if the app is overloaded. Removing it from the Service might give it time to recover.

## Self-repairing apps with liveness probes

Readiness probes isolate failed Pods from the Service load balancer, but they don't take action to repair the app. 

For that you can use a liveness probe which will restart the Pod with a new container if the probe fails:

- [deployment-with-liveness.yaml](specs/whoami/update2/deployment-with-liveness.yaml) - adds a liveness check; this one uses the same test as the readiness probe

You'll often have the same tests for readiness and liveness, but the liveness check has more significant consequences, so you may want it to run less frequently and have a higher failure threshold.

📋 Deploy the update in `labs/kubernetes/containerprobes/specs/whoami/update2` and wait for the Pods with label `update=liveness` to be ready.

<details>
  <summary>Not sure how?</summary>

```
kubectl apply -f labs/kubernetes/containerprobes/specs/whoami/update2

kubectl wait --for=condition=Ready pod -l app=whoami,update=liveness
```

</details><br/>

📋 Now trigger a failure in one Pod and watch to make sure it gets restarted.

<details>
  <summary>Not sure how?</summary>

```
curl --data '503' http://localhost:8010/health

kubectl get po -l app=whoami --watch
```

</details><br/>

> One Pod will become ready 0/1 -then it will restart, and then become ready 1/1 again.

Check the endpoint and you'll see both Pod IPs are in the Service list. When the restarted Pod passed the readiness check it was added back.

Other types of probe exist, so this isn't just for HTTP apps. This Postgres Pod spec uses a TCP probe and a command probe:

- [products-db.yaml](specs/products-db/products-db.yaml) - has a readiness probe to test Postgres is listening and a liveness probe to test the database is usable

___

## Lab

Adding production concerns is often something you'll do after you've done the initial modelling and got your app running. So your task is to add container probes to the Random Number Generator API. Start by running it with a basic spec:

```
kubectl apply -f labs/kubernetes/containerprobes/specs/rngapi
```

Try the app:

```
curl -i http://localhost:8040/rng
```

You'll see it fails after a couple of tries and never comes back online. There's a `/healthz` endpoint you can use to check that. Your goals are:

- run 5 replicas and ensure traffic only gets sent to healthy Pods
- restart Pods if the app in the container fails

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___
## Cleanup

```
kubectl delete all -l kubernetes.courselabs.co=containerprobes
```