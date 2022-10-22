# Lab Solution

From the _Instances_ blade in the Portal you can choose an instance to open, and from the VM instance page you have the _Connect_ option:

- choose _RDP_
- select the _Load balancer public IP address_
- download the RDP file, open it in your client and connect with admin credentials

When you're connected, open PowerShell and run:

```
# print all the Windows Services:
Get-Service

# then stop the IIS WWW Publishing Service:
Stop-Service w3svc
```

You can confirm the web server is no longer running on that instance:

```
# this will show a connection refused error:
curl.exe localhost
```

On your host machine, browse to the PIP address with repeated GET requests:

```
curl http://<pip>
```

You'll only see responses from the instance which you didn't choose for stopping the web server.

In the Portal you can't see that health probes are failing. This is a basic SKU load balancer, and that detail only gets provided for more advanced SKUs.