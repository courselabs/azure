# Lab Solution

The appsettings.json file has this setting:

```
{
  "App": {
    "Environment": "DEV"
  }
}
```

You access that in code using the key `App:Environment`. You can  use that as the name of an environment variable too, but not all platforms like having a colon in the name so you can also use `App__Environment`.

You'll see that in the [Dockerfile](/src/simple-web/Dockerfile), and this is how you override the setting in the image for one container:

```
docker run -d -p 8084:80 -e App__Environment=PROD simple-web 
```

> You need to use a different port, because only one container can listen on each port

Browse to http://localhost:8084/ and you'll see the updated value.