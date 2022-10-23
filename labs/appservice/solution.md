# Lab Solution

The home page content is in `src/WebForms/WebApp/Default.aspx` - here's an updated file you can use:

- [lab/Default.aspx](labs/appservice/lab/Default.aspx) - changes the HTML content

Copy the new file over the original:

```
cp labs/appservice/lab/Default.aspx src/WebForms/WebApp/
```

Commit the changes:

```
git add src/WebForms/WebApp/Default.aspx

git commit -m 'Homepage update'
```

And push to your remote:

```
git push webapp main
```

You'll see the build output again, and the new content should be live in a minute or two.