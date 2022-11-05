# Lab Solution

When you work with a lot of Kubernetes clusters each with lots of namespaces, it gets very difficult to manage them.

There's a great tool called [kubectx](https://kubectx.dev/) which helps with that - it's cross-platform and it lets you easily switch between clusters, along with the partner tool `kubens` for switching namespaces.

They are very useful tools as you use Kubernetes more, you can install them from the [releases](https://github.com/ahmetb/kubectx/releases), or with:

```
# Windows
choco install kubectx kubens

# macOS
brew install kubectx kubens
```

Then use to manage namespaces like this:

```
# list all namespaces
kubens

# switch to pi
kubens pi

# toggle back to the previous namespace:
kubens -
```

I have aliases in all my shells:

```
alias d="docker"
alias k="kubectl"
alias kx="kubectx"
alias kn="kubens"
```

So my typical workflow is:

```
kx <client-cluster>
kn <namespace>
k etc.

kx -
```

> Back to the [exercises](README.md).