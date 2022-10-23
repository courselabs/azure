# Lab Solution

Open the Node.js App Service in the Portal.

Select _Authentication_ from the left menu and hit the _Add identity provider_ button:

- select _Microsoft_ as the identity provider
- select _Any Azure AD directory & personal Microsoft accounts_ in the supported account types
- leave all the options with default values

Click _Add_ then open your Node app URL in a private browser window

> It may take a few minutes for authentication to be configured; if you don't see a login page, wait and try again until you do

The Microsoft login page will appear and you can log in with Azure account - you'll be asked to confirm the app can read account details. There are no changes to the code here, we've just bolted authentication on to the side.

After you log in browse to /user on the site - you'll see Azure listed as the IdP (_aad_ = Azure Active Directory) and your Azure Account email address.
