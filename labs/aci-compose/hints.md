# Lab Hints

Remember the ACI YAML file is a desired-state approach, so if you want to create a second instance you can't just deploy the same YAML file again. The model can stay the same except for one field which needs to be unique between ACI resources.

When you get two instances running, think about how you have to access them and what Azure services are available to help you manage the traffic.

> Need more? Here's the [solution](solution.md).