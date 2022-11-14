# Lab Solution

In the Portal for the SignalR Service open _Live trace settings_ and select:

- _Enable Live Trace_ and tick:
- _Connectivity logs_
- _Messaging logs_ 

Then save.

Click _Open Live Trace Tool_ and open some more browser windows to the chat app. Send some messages through the app - you'll see all the infromation received and sent by the hub.

If you could connect to SignalR and you knew the hub name and message format, you could easily send a message to all connected users. That's why Managed Identity is such a good idea :)