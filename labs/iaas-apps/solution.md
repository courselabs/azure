# Lab Solution

This app deployment has been manual so far, so we can complete the setup using the Portal:

- set a DNS name for the Public IP address
- add an inbound rule to the Network Security Group

The NSG rule needs to be a higher priority than the defaults, and it should allow traffic to port 80 from the Internet.

When you've added some details through the app, you can check the database in the Portal too:

- open Query editor
- sign in - you'll need to add your client IP address to the firewall
- select all from the `Prospects` table and you should see your data
