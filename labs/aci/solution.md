# Lab Solution

You can use the `az` command with the multi-arch image tag and set the OS:

```
az container create -g labs-aci --name simple-web-win --image courselabs/simple-web:6.0 --ports 80 --os Windows --dns-name-label <aci-win-dns>
```

ACI creates a Linux container if you don't specify the OS, so if you use the Windows image tag without telling ACI that it's Windows you'll get an error:

```
# this will error saying the container OS doesn't match the image OS
az container create -g labs-aci --name simple-web-win2 --image courselabs/simple-web:6.0-windows-amd64 --ports 80 --dns-name-label <aci-win-dns2>
```

So you need to set the OS too:

```
az container create -g labs-aci --name simple-web-win2 --image courselabs/simple-web:6.0-windows-amd64 --ports 80 --os Windows --dns-name-label <aci-win-dns2>
```

There's no ARM64 support in ACI, but the image CPU isn't verified when the container is created. ACI will let you run a container:

```
az container create -g labs-aci --name simple-web-arm --image courselabs/simple-web:6.0-linux-arm64 --ports 80 --dns-name-label <aci-arm-dns>
```

But the container exits as soon as it starts. Check the logs and you'll see a message about the exec format - this tells you there is a CPU mismatch between the compiled binary and the runtime:

```
az container logs -g labs-aci -n simple-web-arm
```

Returns:

*standard_init_linux.go:228: exec user process caused: exec format error*
