# Lab Suggestions

This is a bit fiddly - you need to create a listener which is set for `multi-site`. Then when you add rules using that listener you get the option to add a path, which means you can have one rule for `domain.com` and another for `domain.com/static`, each served from different backends.