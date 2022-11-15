# Lab Suggestions

This is not really blue-green. The slots can't be swapped because each revision of the API is tied to a particular slot. APIM is more nuanced than blue-green because you can have multiple revisions and multiple versions of an API live at the same time. For that you can deploy a new slot each time in App Service, but you need to make sure you don't swap them, and the traffic management is done with APIM.
