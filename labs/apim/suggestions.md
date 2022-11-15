# Lab Suggestions

## _customize the text of emails_

You can do this from the _Notification templates_ menu - you can't alter which emails get sent or when, but you can change the content.

## _automate APIM configuration_

There's so much to configure that APIM has it's own approach for this - it hosts a Git repository with all the configuration. You can clone that repo and then upload it to GitHub or Azure DevOps to keep your own copy. Then you can deploy from that repo to an empty APIM instance to restore the setup.

## _is your backend web app still publicly available?_

Yes! APIM adds a front-end with policies to call the backend, but it doesn't disable access to the backend. You need to do that to stop people bypassing APIM and all your policies - you could add a network restriction so only the APIM IP address is allowed access.