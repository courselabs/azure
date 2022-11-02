# Lab Solution


Print the help text for creating a certificate:

```
az keyvault certificate create --help
```

You need to provide a certificate policy - every vault is created with a default policy. Save that as a JSON file which you can use as a template:

```
az keyvault certificate get-default-policy > labs/keyvault/lab/default-policy.json
```

I've edited the default policy to set the certificate values we want in:

- [lab/lab-policy.json](/labs/keyvault/lab/lab-policy.json)

Now we can create the certificate using that custom policy:

```
az keyvault certificate create -n lab-cert -p @labs/keyvault/lab/lab-policy.json --vault-name <kv-name> 
```

It will take a while to create and the output shows the Certificate Signing Request (CSR), but not the certificate details. The `az keyvault certificate download` command only downloads the public key. To export both public and private keys you need to download a secret:

```
az keyvault secret download -f lab-cert.pfx --name lab-cert --vault-name <kv-name> 
```

> This is a PFX file which you can separate into public and private certificate files using tools like OpenSSL.

Note that certificates are not visible as normal secrets in the KeyVault:

```
# no certificate shown here
az keyvault secret list --vault-name <kv-name> 
```