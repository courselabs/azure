# Lab Solution

There are some quirks to SQL support in Cosmos.

This is fine:

```
SELECT *
FROM AssetContext
```

But this fails:

```
SELECT Id
FROM AssetContext
```

because identifiers need to be anchored to a table:

```
SELECT c.Id
FROM AssetContext c
```

The asset type list is a selection based on Discriminator:

```
SELECT c.Id, c.Description
FROM AssetContext c
WHERE c.Discriminator = "AssetType"
```

And the location count uses a string comparison:

```
SELECT COUNT(c.Id)
FROM AssetContext c
WHERE c.Discriminator = "Location"
AND c.PostalCode LIKE '%1%'
```

You can try COUNT(*) but it will fail for the same reason, that Cosmos wants identifiers to be explicitly anchored.
