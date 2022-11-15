Welcome to the Azure labs.

These are hands-on resources to help you learn Azure.

## Pre-reqs

 - Create an Azure account
 - [Set up the AZ command line, Git and Docker](./setup/README.md) 
 - Download your repo
    - Open a terminal (PowerShell on Windows; any shell on Linux/macOS) 
    - Run: `git clone https://github.com/courselabs/azure.git`
     - Open the folder: `cd azure`
- _Optional_
    - Install [Visual Studio Code](https://code.visualstudio.com) (free - Windows, macOS and Linux) to browse the repo and documentation

## Azure Quickstart

_Resource Groups and Virtual Machines_

- [Signing In](/labs/signin/README.md)
- [Regions and Resource Groups](/labs/resourcegroups/README.md)
- [Virtual Machines](/labs/vm/README.md)
- [VMs as Linux Web servers](/labs/vm-web/README.md)
- [VMs as Windows dev machines](/labs/vm-win/README.md)
- [Automating VM configuration](/labs/vm-config/README.md)

_SQL Databases and ARM_

- [SQL Server](/labs/sql/README.md)
- [SQL Server VMs](/labs/sql-vm/README.md)
- [Deploying database schemas](/labs/sql-schema/README.md)
- [Automation with ARM](/labs/arm/README.md)
- [Automation with Bicep](/labs/arm-bicep/README.md)

_App Deployment with IaaS_

- [IaaS app deployment](/labs/iaas-apps/README.md)
- [Automating IaaS app deployment](/labs/iaas-bicep/README.md)
- [Creating and using VM images](/labs/vm-image/README.md)
- [Scaling with VM Scale Sets](/labs/vmss-win/README.md)
- [Provisiong Scale Sets with cloud-init](/labs/vmss-linux/README.md)

_App Service_

- [App Service for web applications](/labs/appservice/README.md)
- [App Service for static web apps](/labs/appservice-static/README.md)
- [App Service for distributed apps](/labs/appservice-api/README.md)
- [App Service configuration and administration](/labs/appservice-config/README.md)
- [App Service CI/CD](/labs/appservice-cicd/README.md)

_Project_

- [Project 1: Lift and Shift](/projects/lift-and-shift/README.md)

## Storage and Communication

_Storage Accounts_

- [Storage Accounts](/labs/storage/README.md)
- [Blob storage](/labs/storage-blob/README.md)
- [File shares](/labs/storage-files/README.md)
- [Using storage for static web content](/labs/storage-static/README.md)
- [Working with table storage](/labs/storage-table/README.md)

_Cosmos DB_

- [Cosmos DB](/labs/cosmos/README.md)
- [Cosmos DB with the Mongo API](/labs/cosmos-mongo/README.md)
- [Cosmos DB with the Table API](/labs/cosmos-table/README.md)
- [Cosmos DB performance and billing](/labs/cosmos-perf/README.md)

_KeyVault and Virtual Networks_

- [KeyVault](/labs/keyvault/README.md)
- [Virtual Networks](/labs/vnet/README.md)
- [Securing KeyVault Access](/labs/keyvault-access/README.md)
- [Securing VNet Access](/labs/vnet-access/README.md)
- [Securing apps with KeyVault and VNet](/labs/vnet-apps/README.md)

_Events and Messages_

- [Service Bus Queues](/labs/servicebus/README.md)
- [Service Bus Topics](/labs/servicebus-pubsub/README.md)
- [Event Hubs](/labs/eventhubs/README.md)
- [Eveng Hubs partitioned consumer](/labs/eventhubs-consumers/README.md)
- [Azure Cache for Redis](/labs/redis/README.md)

_Project_

- [Project 2: Distributed App](/projects/distributed/README.md)

## Compute and Containers

_Docker and Azure Container Instances_

- [Docker 101](/labs/docker/README.md)
- [Docker images and Azure Container Registry](/labs/acr/README.md)
- [Azure Container Instances](/labs/aci/README.md)
- [Distributed apps with Docker Compose](/labs/docker-compose/README.md)
- [Distributed apps with ACI](/labs/aci-compose/README.md)

_Kubernetes_

- [Nodes](/labs/kubernetes/nodes/README.md)
- [Pods](/labs/kubernetes/pods/README.md)
- [Services](/labs/kubernetes/services/README.md)
- [Deployments](/labs/kubernetes/deployments/README.md)
- [ConfigMaps](/labs/kubernetes/configmaps/README.md)
- [Azure Kubernetes Service](/labs/aks/README.md)

_Intermediate Kubernetes_

- [PersistentVolumes](/labs/kubernetes/persistentvolumes/README.md)
- [AKS PersistentVolumes](/labs/aks-persistentvolumes/README.md)
- [Ingress](/labs/kubernetes/ingress/README.md)
- [AKS with Application Gateway Ingress Controller](/labs/aks-ingress/README.md)
- [Container Probes](/labs/kubernetes/containerprobes/README.md)
- [Troubleshooting](/labs/kubernetes/troubleshooting/README.md)

_AKS Integration_

- [Namespaces](/labs/kubernetes/namespaces/README.md)
- [Secrets](/labs/kubernetes/secrets/README.md)
- [AKS with KeyVault secrets](/labs/aks-keyvault/README.md)
- [Helm](/labs/kubernetes/helm/README.md)
- [Securing AKS apps with KeyVault and VNet](/labs/aks-apps/README.md)

_Project_

- [Project 3: Containerized App](/projects/conatinerized/README.md)

## Serverless and App Management

_Azure Functions_

- [HTTP trigger](/labs/functions/http/README.md)
- [Timer trigger & blob output](/labs/functions/timer/README.md)
- [Blob trigger & SQL output](/labs/functions/blob/README.md)
- [Service Bus trigger & multiple outputs](/labs/functions/servicebus/README.md)
- [RabbitMQ trigger & blob output](/labs/functions/rabbitmq/README.md)
- [CosmosDB trigger & output](/labs/functions/cosmos/README.md)

_Durable Functions_

- [CI/CD for Azure Functions](/labs/functions/cicd/README.md)
- [Durable functions](/labs/functions-durable/chained/README.md)
- [Fan-out fan-in pattern](/labs/functions-durable/fan-out/README.md)
- [Human interaction pattern](/labs/functions-durable/human/README.md)
- [Azure SignalR Service](/labs/signalr/README.md)
- [SignalR functions output](/labs/functions/signalr/README.md)

_API Management_ 

- [API Management](/labs/apim/README.md)
- [Mocking APIs](/labs/apim-mock/README.md)
- [Securing APIs with policies](/labs/apim-policies/README.md)
- [Versioning APIs for breaking changes](/labs/apim-versioning/README.md)

_Web Application Firewall & CDN_

- [Application Gateway & WAF](/labs/appgw/README.md)
- [Front Door & WAF](/labs/frontdoor/README.md)

_Monitoring_

- Instrumentation in code with App Insights
- Using Azure Monitor with App Insights and Container Insights
- Distributed tracing & Application Map
- Centralized log collection & Log Analytics
- Configuring alerts and notifications

_Project_

- Project 4: Serverless API


#### Credits

Created by [@EltonStoneman](https://twitter.com/EltonStoneman) ([sixeyed](https://github.com/sixeyed)): Freelance Consultant and Trainer. Author of [Learn Docker in a Month of Lunches](https://www.manning.com/books/learn-docker-in-a-month-of-lunches), [Learn Kubernetes in a Month of Lunches](https://www.manning.com/books/learn-kubernetes-in-a-month-of-lunches) and [many Pluralsight courses](https://pluralsight.pxf.io/c/1197078/424552/7490?u=https%3A%2F%2Fwww.pluralsight.com%2Fauthors%2Felton-stoneman).


Photo by <a href="https://unsplash.com/@_bahador?utm_source=unsplash&utm_medium=referral&utm_content=creditCopyText">Bahador</a> on <a href="https://unsplash.com/s/photos/cloud?utm_source=unsplash&utm_medium=referral&utm_content=creditCopyText">Unsplash</a>
  