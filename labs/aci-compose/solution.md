# Lab Solution

The YAML file contains the ACI name, so if you redeploy it - nothing happens :) There are no updates to the named instance, so it gets left as-is.

You can try overriding the name in the CLI:

```
# this will fail:
az container create -g labs-aci-compose -n assetmanager2 --file labs/aci-compose/assetmanager-aci.yaml
```

That doesn't work, so you need to create a copy of the YAML with a new name (or no name) and deploy that:

- [lab/assetmanager2-aci.yaml](/labs/aci-compose/lab/assetmanager2-aci.yaml) - same spec except for the name

You need to **edit that file** and add your connection details, then you can deploy:

```
az container create -g labs-aci-compose -n assetmanager2 --file labs/aci-compose/lab/assetmanager2-aci.yaml
```

Browse to the IP address for the new instance - it's the same app. Open the Azure Files share and you'll see a new lockfile, each instance writes its own.

Separate IP addresses isn't much help unless your DNS service supports load-balancing between them. Alternatively you could add a DNS name to each and create a Traffic Manager profile to distribute the load.