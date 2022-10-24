# Lab Hints

You don't need to edit JSON files and push changes to have different configuration settings in different slots. You can manage configuration in the Portal for that.

This is a .NET 6 app, and the native JSON config format is nested. The App Service configuration doesn't support that so you need to flatten out the hieararchy, using double underscores `__` to denote the levels.

So this setting in JSON:

```
{
    "app" : {
        "config": {
            "setting" : 4
        }
    }
}
```

Would be represented as a setting with the key `app__config__setting`.

> Need more? Here's the [solution](solution.md).