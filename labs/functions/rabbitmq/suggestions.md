# Lab Suggestions

The Bitnami RabbitMQ marketplace offering is really just a custom VM image. You could find the image name and create a VM directly with the standard `az vm create` command. Then use the script action to capture the RabbitMQ username and password from the file.

Then there is the [rabbitmqadmin](https://www.rabbitmq.com/management-cli.html) CLI which you can use to create queues.