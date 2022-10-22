# Lab Hints

When your VMSS has scaled down to the minimum 2 instances, you can pick one to take offline. The Portal will help you to connect.

If you're not familiar with Windows Services, they're background processes like Linux daemons, which are intended to run all the time the machine is running. You can manage them with PowerShell using commands like [this]() and [this](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.management/stop-service?view=powershell-7.2).

When your chosen server stops responding to HTTP requests the health probe will fail and all requests should be served by the remaining VM. In the Portal you might not find out that the health probe has failed.

> Need more? Here's the [solution](solution.md).