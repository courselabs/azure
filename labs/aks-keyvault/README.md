# AKS with KeyVault Secret Storage

Kubernetes has a pluggable storage architecture - the Container Storage Interface (CSI). Different types of storage can be connected to a Kubernetes cluster and surfaced as volumes inside Pod containers.

An add-on for AKS enables KeyVault as a storage provider. You can store sensitive configuration files in KeyVault and mount them in container folders without the data being available anywhere else in Kubernetes.

## Reference

- [Secrets Store CSI Driver](https://secrets-store-csi-driver.sigs.k8s.io)

- [Use the Azure Key Vault Provider for Secrets Store CSI Driver in an AKS cluster](https://docs.microsoft.com/en-us/azure/aks/csi-secrets-store-driver)

## Create the AKS cluster

Start with a new Resource Group for the lab:

```
az group create -n labs-aks-keyvault --tags courselabs=azure -l eastus
```

Remember that KeyVault access is restricted so we'll need a security principal the AKS nodes can use when connecting to other Azure services.

📋 Create a new AKS cluster with two nodes, setting flags to use a Managed Identity and enable the KeyVault add-on.

<details>
  <summary>Not sure how?</summary>

```
az aks create -g labs-aks-keyvault -n aks05 --node-count 2 --enable-addons azure-keyvault-secrets-provider --enable-managed-identity -l eastus
```

</details><br/>

When the cluster is created, open it in the Portal. Do you see anything in the UI which tells you there is KeyVault integration?

Additional cluster components like CSI drivers usually run as Pods inside Kubernetes, but they're segragated in a Kubernetes _namespace_, which is a way of isolating workloads (similar to Resource Groups in Azure).

Connect to your new cluster and print the Pods in the Kubernetes system namespace:

```
az aks get-credentials -g labs-aks-keyvault -n aks05 --overwrite-existing

kubectl get pods --namespace kube-system -l app=secrets-store-csi-driver
```

You should see two Pods with names starting with `aks-secrets-store-csi-driver-`. If you need to debug any issues connecting AKS with KeyVault then you can check the logs for those Pods. 

## Create the KeyVault and authorize AKS

We don't need anything special in the KeyVault, the default options are fine:

```
az keyvault create -g labs-aks-keyvault -n <kv-name>
```

Now we need to get the ID for the Managed Identity AKS is using, and allow the ID to use the KeyVault (we covered this in the [KeyVault access lab](labs/keyvault-access/README.md)):

```
# print the identity ID:
az aks show -g labs-aks-keyvault -n aks05 --query addonProfiles.azureKeyvaultSecretsProvider.identity.clientId -o tsv

# add a policy to allow access to the identity:
az keyvault set-policy --secret-permissions get --spn '<identity-id>' -n <kv-name>
```

> You don't specifically link an AKS cluster to one KeyVault.

In your Kubernetes model you specify the details of the KeyVault secrets you want to mount. AKS tries to access the secrets when you deploy your app, and as long as the ID has access to the KeyVault they can be read and injected into the container filesystem.

## Create and model KeyVault secrets

The KeyVault details are modelled in a special type of resource - the _SecretProviderClass_. This isn't a core Kubernetes resource, it's added to the cluster when you install the KeyVault add-on:

- [secretProviderClasses/keyVault.yaml](/labs/aks-keyvault/specs/secretProviderClasses/keyVault.yaml) - contains the KeyVault name and Azure [tenant ID](https://learn.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-how-to-find-tenant)

This is a fine-grained approach to secrets, you need to explicitly set which KeyVault objects get made available in the volume mount:

- `objectName` is the name of the secret (or certificate) in KeyVault
- `objectType` is the type of the KeyVault item
- `objectAlias` is the filename to use when the object is surfaced in the volume mount.

> This example expects a KeyVault secret called `configurable-secrets`; it will make the contents of that secret available in a file called `secret.json`

Let's create that secret by uploading the local JSON file [configurable-secret.json](/labs/aks-keyvault/secrets/configurable-secret.json):

```
az keyvault secret set --name configurable-secrets --file labs/aks-keyvault/secrets/configurable-secret.json --vault-name <kv-name>
```

Open the KeyVault in the Portal and check that your secret is there. Secrets don't need to be single values, they can be a multi-line string (up to a maximum size of 25Kb).

📋 **Update the [keyVault.yaml](/labs/aks-keyvault/specs/secretProviderClasses/keyVault.yaml) file with your details** and deploy the secret provider class into your AKS cluster.

<details>
  <summary>Not sure how?</summary>

Get your Azure tenant ID:

```
az account list -o table
```

And AKS identity ID:

```
az aks show -g labs-aks-keyvault -n aks05 --query addonProfiles.azureKeyvaultSecretsProvider.identity.clientId -o tsv
```

Replace the values `<tenant-id>`, `<identity-id>` and `<kv-name>` in your YAML file and deploy:

```
kubectl apply -f labs/aks-keyvault/specs/secretProviderClasses/keyVault.yaml
```

</details><br />

You can check the SCP exists in the cluster, but the details don't tell you much. But it's there and ready to mount volumes from the KeyVault secret.

## Deploy an app using KeyVault volumes

It's back to the good old configurable app, using this spec:

- [configurable/deployment.yaml](labs/aks-keyvault/specs/configurable/deployment.yaml) - uses a CSI volume with the secrets store driver, specifying the KeyVault SCP and mounting it into the container at `/app/secrets`

Run the app - this will create the Deployment and a LoadBalancer Service:

```
kubectl apply -f labs/aks-keyvault/specs/configurable
```

Check the Pod starts correctly and goes into the running state:

```
kubectl get po -l app=configurable  --watch
```

Then check that the JSON in the KeyVault secret has been loaded into the container filesystem in the expected place:

```
kubectl exec deploy/configurable -- cat /app/secrets/secret.json
```

Browse to the app at the LoadBalancer IP - you should see the secret values in the page.

## Lab

ConfigMaps and Secrets in Kubernetes can be updated, and any Pods mounting them have the contents of the volume updated too. It can take a few minutes because the data it cached, and there's no guarantee a changed config file will get picked up by the application (some only read config files once when the app starts). 

What about the CSI secrets store? Update the contents of the KeyVault secret - does it flow through into the configurable app?

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

You can delete the RG for this lab to remove all the resources, including the storage.

```
az group delete -y --no-wait -n labs-aks-keyvault
```

Now change your Kubernetes context back to your local Docker Desktop:

```
kubectl config use-context docker-desktop
```