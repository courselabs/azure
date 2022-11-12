# Lab Solution

List the Pods with wide output and you'll see the internal IP addresses:

```
kubectl get pods -o wide
```

> The Pod addresses are all in the range you specified for the subnet. 

This is because we used the Azure network plugin, and it means individual Pods are addressable across the vnet from services outside of Kubernetes.

Open the Application Gateway in the Azure Portal:

- the _Frontend IP configurations_ shows the public IP address with your DNS name
- under _Health probes_ you'll see a probe with a name like `pb-default-whoami-internal-http-whoami` which shows how the Pods are tested for health
- under _Backend pools_ you'll see an entry like  `pool-default-whoami-internal-http-bp-80`
- select that pool and you'll see your Pod IP addresses

Application Gateway is running in the same vnet as AKS, so it can reach the Pods using their internal IP addresses.