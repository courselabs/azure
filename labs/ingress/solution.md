# Lab Solution

The quickest way to scale up or down is with Kubectl:

```
kubectl scale deploy/simple-web --replicas 4
```

> Be careful with this approach, because it means your running application is different from your model in source control.

List the Pods with wide output and you'll see the internal IP addresses:

```
kubectl get pods -o wide
```

Refresh your browser and you should see the responses all come from different Pods.

Find the Application Gateway in the Azure Portal - it will be called `appgw`, in the AKS `MC_` Resource Group:

- the _Frontend IP configurations_ shows the public IP address with your DNS name
- under _Health probes_ you'll see a probe for simple-web which shows how the Pods are tested for health
- under _Backend pools_ you'll see an entry like  `pool-default-simple-web-http-bp-80`
- select that pool and you'll see your Pod IP addresses

Application Gateway is configured in the same vnet as AKS (all created for you), so it can reach the Pods using their internal IP addresses.