# Lab Hints

The Deployment API spec lists the field you need to configure the number of Pods. We didn't set it in the original Deployment YAML but it defaults to 1, which is why we have one Pod.

The environment name shown on the site is defined in the ConfigMap. You might be able to guess what will happen when you update it, if you look at how the data is surfaced in the Deployment.

> Need more? Here's the [solution](solution.md).