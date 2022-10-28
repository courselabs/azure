# Lab Solution

These are standard Mongo Shell commands, which you can use in the Portal or the VS Code extension.

Find all the collections:

```
use('AssetsDb');
db.getCollectionNames();
```

You'll see _Locations_ and _AssetTypes_ and _Assets_ are separate collections. This is different from the NoSQL API in Cosmos where the documents are stored in the same container, using the _Discriminator_ field to identify object type.

Print all the locations:

```
db.Locations.find().pretty();
```

Insert a new one:

```
db.Locations.insertOne({
    "AddressLine1": "1 Parliament Place",
    "Country": "Singapore",
    "PostalCode": "178880"
});
```

Refresh the app and you should see the new data.