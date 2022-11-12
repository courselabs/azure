# Lab Suggestions

This depends a lot on what you want to do with the translated documents. Storing them in the same collection means you can easily query for all documents with translations (as the new document stores the ID of the original document) - without having to query across collections.

There is no way to filter documents in the trigger to prevent it being called when if the translations are inserted into the same container.

The alternative is to store translations in a separate container, e.g. `posts-es`. That would prevent the trigger firing again when the translation is inserted, but it would make querying harder (and more expensive).

And consider the infinite loop. If you decided to _add_ the translated fields to the original document instead of creating a new one so they looked like this:

```
{
    "id": "897",
    "message": "hello",
    "lang" : "en",
    "translatedMessage" : "hola",
    "translatedLang" : "es",
    "translatedTimestamp": 221112024412
}
```

Your logic would need to check to see if the translated message was already there. If not and it set the translation with a new timestamp each time, then it would keep firing...