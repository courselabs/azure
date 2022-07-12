# Lab Solution

New projects need a type, and it's good to set an output directory:

```
dotnet new console -o labs-signin
```

Check the folder to see the new project:

```
cd labs-signin

ls
```

There's already a `Program.cs`; click the _Upload/Download Files_ icon in the shell toolbar and upload your own file from the lab folder.

> The upload tool always saves files in your home directory.

Now you need to move it:

```
mv ../Program.cs .
```

Check the source file and run the app:

```
cat Program.cs

dotnet run
```


