# Configuring Apps with Secrets

ConfigMaps are flexible for pretty much any application config system, but they're not suitable for sensitive data. ConfigMap contents are visible in plain text to anyone who has access to your cluster.

For sensitive information Kubernetes has [Secrets](https://kubernetes.io/docs/concepts/configuration/secret/). The API is very similar - you can surface the contents as environment variables or files in the Pod contianer - but there are additional safeguards around Secrets.

## API specs

- [Secrets](https://kubernetes.io/docs/reference/generated/kubernetes-api/v1.20/#secret-v1-core)

<details>
  <summary>YAML overview</summary>

## Secrets and Pod YAML - environment variables

Secret values can be base-64 encoded and set in YAML data:

```
apiVersion: v1
kind: Secret
metadata:
  name: configurable-secret-env
data:
  Configurable__Environment: cHJlLXByb2QK
```

The metadata is standard - you'll reference the name of the Secret in the Pod spec to load settings.

* `data` - list of settings as key-value pairs, separated with colons and with values base-64 encoded

In the Pod spec you add a reference:

```
spec:
  containers:
    - name: app
      image: sixeyed/configurable:21.04
      envFrom:
        - secretRef:
            name: configurable-secret-env
```

* `envFrom` - load all the values in the source as environment variables

</details><br />

## Creating Secrets from encoded YAML

Modelling your app to use Secrets is the same as with ConfigMaps - loading environment variables or mounting volumes.

In the container environment, Secret values are presented as plain text.

Start by deploying the configurable app using ConfigMaps:

```
kubectl apply -f labs/kubernetes/secrets/specs/configurable
```

📋 Check the details of a ConfigMap and you can see all the values in plain text.

<details>
  <summary>Not sure how?</summary>

```
kubectl get configmaps

kubectl describe cm configurable-env
```

> That's why you don't want sensitive data in there.

</details><br />

This YAML creates a Secret from an encoded value, and loads it into environment variables in a Deployment:

- [secret-encoded.yaml](specs/configurable/secrets-encoded/secret-encoded.yaml) - uses `data` with encoded values
- [deployment-env.yaml](specs/configurable/secrets-encoded/deployment-env.yaml) - loads the Secret into environment variables

```
kubectl apply -f labs/kubernetes/secrets/specs/configurable/secrets-encoded
```

> Browse to the website and you can see the plain-text value for `Configurable:Environment`

## Creating Secrets from plaintext YAML

Encoding to base-64 is awkward and it gives you the illusion your data is safe. Encoding is not encryption, and you can easily decode base-64.

If you want to store sensitive data in plaintext YAML, you can do that instead. You'd only do this when your YAML is locked down:

- [secret-plain.yaml](specs/configurable/secrets-plain/secret-plain.yaml) - uses `stringData` with values in plain text
- [deployment-env.yaml](specs/configurable/secrets-plain/deployment-env.yaml) - loads the Secret into environment variables

```
kubectl apply -f labs/kubernetes/secrets/specs/configurable/secrets-plain
```

> Refresh the site and you'll see the updated config value

## Working with base-64 Secret values

Secrets are always surfaced as plaintext inside the container environment.

<details>
  <summary>They **may** be encrypted in the Kubernetes database</summary>

But that is not the default setup. You can also integrate Kubernetes with third-party secure stores like Hashicorp Vault and Azure KeyVault (the [Secrets CSI driver](https://secrets-store-csi-driver.sigs.k8s.io) and [external-secrets](https://github.com/external-secrets/kubernetes-external-secrets) projects are popular options).

</details><br/>

Kubectl always shows Secrets encoded as base-64, but that's just a basic safety measure.

_Windows doesn't have a base64 command, so run this PowerShell script **if you're on Windows**:_

```
Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process -Force

. ./scripts/windows-tools.ps1
```

> This only affects your current PowerShell session, it doesn't make any permanent changes to your system.

You can fetch the data item from a Secret, and decode it into plaintext:

```
kubectl describe secret configurable-env-plain

kubectl get secret configurable-env-plain -o jsonpath="{.data.Configurable__Environment}"

kubectl get secret configurable-env-plain -o jsonpath="{.data.Configurable__Environment}" | base64 -d
```

## Creating Secrets from files

Some organizations have separate configuration management teams. They have access to the raw sensitive data, and in Kubernetes they would own the management of Secrets. 

The product team would own the Deployment YAML which references the Secrets and ConfigMaps. The workflow is decoupled, so the DevOps team can deploy and manage the app without having access to the sensitive data.

Play the config management team with access to secrets on your local disk:

- [configurable.env](secrets/configurable.env ) - a .env file for loading environment variables
- [secret.json](secrets/secret.json) - a JSON file for loading as a volume mount

📋 Create secrets from the files in `labs/kubernetes/secrets/secrets`.

<details>
  <summary>Not sure how?</summary>

```
kubectl create secret generic configurable-env-file --from-env-file ./labs/kubernetes/secrets/secrets/configurable.env 

kubectl create secret generic configurable-secret-file --from-file ./labs/kubernetes/secrets/secrets/secret.json
```

</details><br/>

Now play the DevOps team, deploying the app using the secrets that already exist:

- [deployment.yaml](specs/configurable/secrets-file/deployment.yaml) - references those Secrets

```
kubectl apply -f ./labs/kubernetes/secrets/specs/configurable/secrets-file
```

> Browse to the app and now you can see another config source - the `secret.json` file

## Lab

Configuration loaded into volume mounts is managed by Kubernetes. If the source ConfigMap or Secret changes, Kubernetes pushes the change into the container filesystem.

But the app inside the Pod might not check the mount for file updates, so as part of a configuration change you would need to force a rollout of the Deployment to recreate the Pods and load the new config.

That isn't a great option - it makes for a multi-stage update process, with the risk that steps get forgotten. Come up with an alternative approach so when you apply changes to a Secret in YAML, the Deployment rollout happens as part of the same update.

> Stuck? Try [hints](hints.md) or check the [solution](solution.md).

___

## Cleanup

```
kubectl delete all,cm,secret -l kubernetes.courselabs.co=secrets
```