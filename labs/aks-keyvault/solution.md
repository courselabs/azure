# Lab Solution

I have an updated JSON file here:

- [solution/configurable-secret.json](/labs/aks-keyvault/solution/configurable-secret.json)

Use that to set a new version of the KeyVault secret:

```
az keyvault secret set --name configurable-secrets --file labs/aks-keyvault/solution/configurable-secret.json --vault-name <kv-name>
```

And confirm the changes are set as the current version:

```
az keyvault secret show --name configurable-secrets --vault-name <kv-name>
```

Refresh the app now and the changes won't show. 

Check the filesystem:

```
kubectl exec deploy/configurable -- cat /app/secrets/secret.json
```

This won't show the changes either. There is an [auto-rotation feature](https://secrets-store-csi-driver.sigs.k8s.io/topics/secret-auto-rotation.html) but it's not enabled by default, so the secret details are only read when the Pod is created.

Force a rollout to replace the Pod with a new one:

```
kubectl rollout restart deploy/configurable
```

When the new Pod comes online the app will have the updated configuration.