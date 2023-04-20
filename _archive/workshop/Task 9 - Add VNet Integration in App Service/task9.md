# Task 8 - Add VNet Integration in App Service

At this point, we have deployed a useful web app in our Azure resource group. The App Service (which hosts the API and SPA) is publicly available so that all the registered user in the associated Active Directory can access it from the internet.

However, when we develop web apps for enterprise customers or various government departments, it's a very common requirement to limit the accessibility of the service to a specific group of users. In this task we will choose a simple method and demonstrate some key concept by enabling Azure VNet in our App Service to achieve this goal. However, a complete setup of an enterprise private network will be much more complicated and beyond the scope of this masterclass.

## Topology Changes

Before:
Browser => [Internet] => App Service => [Internet] => Database

After:
Browser => [Internet] => Bastion => VM => [Private Endpoint] => App Service => [Private Endpoint] => Database

## Disable Public Access in App Service

Firstly, let's disable the public access in our web app.

1. Login to your Azure Portal
1. Open the App Service `as-AzureMasterclass-dev`
1. Navigate to `Networking` tab, in the `Inbound Traffic` section, click `Access Restriction`
1. Uncheck `Allow public access` and click `Save` on the top action bar

After this change, when you try to open the our web app URL https://as-azuremasterclass-dev.azurewebsites.net in a browser, you will get a `Error 403 - Forbidden` error, which means that public access from internet has been blocked on our App Service.

## Add Virtual Network

Azure Virtual Network (VNet) is one of the key element in building the private network on Azure. For more detailed introduction on Virtual Network, please refer to the [official document](https://learn.microsoft.com/en-us/azure/virtual-network/).

1. Login to your Azure Portal
1. Open the resource group `AzureMasterClass`
1. Click `Create` button on the top action bar
1. Choose `Virtual Network` by using the search function
1. You will see a resource provisioning wizard on first page `Basics`:
   - The Subscription and Resource Group have been automatically filled in
   - Name: `azure-masterclass-vnet`
   - Region: `Australia East`
1. In the second page `IP Addresses`:
   - Add a new subnet with name `masterclass-appservice-inbound-subnet` and IP range `10.0.1.0/24`
1. Leave `Security` and `Tags` pages untouched
1. In `Review + create` page:
   - Optionally, you can download the template before creation by clicking the `Download a template for automation` button
   - Click `Create` button and wait until it's provisioned successfully

## Add Private Endpoint in App Service

Now we can enable access from VNet to our App Service.

1. Login to your Azure Portal
1. Open the App Service `as-AzureMasterclass-dev`
1. Navigate to `Networking` tab, in the `Inbound Traffic` section, click `Private endpoints`
1. Click `Add` on top action bar and choose `Express`
1. In the `Add Private Endpoint` dialog:
   - Name: `masterclass-appservice-vnet-access`
   - Subscription: `Visual Studio Professional Subscription`
   - Virtual Network: `masterclass-vnet`
   - Subnet: `masterclass-appservice-inbound-subnet`
   - Integrate with private DNS zone: `Yes`
   - Click `OK` to add the private endpoint

You will soon notice that the a new IP address `10.0.1.X` will be assigned as `Inbound address` under `Inbound Traffic` section. This means that the App Service has been assigned a IP address within the VNet and are ready to accept incoming request from within the same subnet.

## Add VM

In order to test the access of our web app which has been moved into our new VNet, we need a use a client or machine which is within the same VNet. In real world scenario, this will involve setting up VPN connection from some specific office network to the VNet. In order to simplify the demonstration, we will create a VM and connect it to the same VNet for testing.

To simplify our setup, we will provision a Windows VM because:

- We need a browser to access our React App
- Linux VM requires some [additional steps](https://learn.microsoft.com/en-us/azure/virtual-machines/linux/use-remote-desktop?tabs=azure-cli) to install desktop environment and enable XRDP; while Windows VM supports RDP natively
- Low-spec Windows VMs are about the same price as Linux ones

1. Login to your Azure Portal
1. Open the resource group `AzureMasterClass`
1. Click `Create` button on the top action bar
1. Choose `Virtual Machine` by using the search function
1. You will see a resource provisioning wizard on first page `Basics`:
   - The Subscription and Resource Group have been automatically filled in
   - Virtual machine name: `azure-masterclass-vm-dev`
   - Region: `Australia East`
   - Availability options: `No infrastructure redundancy required`
   - Security type: `Standard`
   - Image: `Windows 10 Pro, version 21H2 - x64 Gen2`
   - VM architecture: `x64`
   - Size: `Standard_B1s - 1vcpu, 1GiB memory`
   - Username: `azuremasterclasstester`
   - Password: ANY STRONG PASSWORD
   - Public inbound ports: `None`
   - Licensing: `I confirm I have an eligible Windows 10/11 license with multi-tenant hosting rights.`
1. Leave `Disks` page untouched
1. In `Networking` page:
   - Virtual Network: `azure-masterclass-vnet`
   - Subnet: `masterclass-appservice-inbound-subnet`
   - Public IP: `None`
   - NIC network security group: `Basic`
   - Public inbound ports: `None`
1. Leave `Management`, `Monitoring`, `Advanced` and `Tags` pages untouched
1. In `Review + create` page:
   - Optionally, you can download the template before creation by clicking the `Download a template for automation` button
   - Click `Create` button and wait until the VM is provisioned successfully

## Add Azure Bastion Service on VM

Because the public access has been disabled on both App Service and the new VM, we need a way to connect to the VM where we can test our App Service. Luckily, Microsoft also provides [Azure Bastion](https://learn.microsoft.com/en-us/azure/bastion/bastion-overview) which allows us to connect to our VM from the browser via Azure Portal. It supports both SSH and RDP and works pretty well on both Windows and Linux VM.

1. Login to your Azure Portal
1. Open the VM `azure-masterclass-vm-dev`
1. Navigate to `Bastion` tab, click `Deploy Bastion`
1. It can take up to 5 mins to create a new Bastion instance

## Test our Web App from the VM

Now it's time to test our App Service from our VM on private network.

1. Login to your Azure Portal
1. Open the VM `azure-masterclass-vm-dev`
1. Navigate to `Bastion` tab, enter user name `azuremasterclasstester` and password
1. Click `Connect`
1. Wait for Windows 10 to finish initialization for the first time
1. Open Microsoft Edge and navigate to https://as-azuremasterclass-dev.azurewebsites.net

You should be able to see our familiar web app again.

## Add new subnets in Virtual Network

In order to allow App Service to connect to our SQL server from our private network , we need a pair of new subnets.

1. Login to your Azure Portal
1. Open the Virtual Network `azure-masterclass-vnet`
1. In the tab `Subnets`:
   - Add a new subnet with name `masterclass-appservice-outbound-subnet` and IP range `10.0.3.0/24`
   - Add a new subnet with name `masterclass-sql-inbound-subnet` and IP range `10.0.4.0/24`

## Bring SQL to our virtual network

1. Login to your Azure Portal
1. Open the Sql Server `sql-AzureMasterclass-dev`
1. Navigate to `Networking` tab
1. In the `Public Access` section, switch toggle `Public network access` to `Disabled`. This will reject all the traffic from internet (including Azure DevOps pipeline)
1. In the `Private Access` tab, click `Create a private endpoint`. This will open a new resource creation wizard `Create a private endpoint`.
1. In the `Basics` tab:
   - The Subscription and Resource Group have been automatically filled in
   - Name: `masterclass-appservice-sql-access`
   - Network Interface Name: `masterclass-appservice-sql-access-nic`
   - Region: `Australia East`
1. In the `Resource` tab:
   - Target sub-resource: `sqlServer`
1. In the `Virtual Network` tab:
   - Virtual network: `azure-masterclass-vnet`
   - Subnet: `masterclass-sql-inbound-subnet`
   - Private IP configuration: `Dynamically allocate IP address`
   - Application security group: leave it as empty
1. In the `DNS` tab:
   - Integrate with private DNS zone: `Yes`
1. Leave `Tags` page untouched
1. In `Review + create` page:
   - Optionally, you can download the template before creation by clicking the `Download a template for automation` button
   - Click `Create` button and wait until the Private Endpoint is provisioned successfully. Several associated resources will also be created in the same resource group hence it could take several minutes to complete.

## Let App Service connect to the Azure SQL through the private endpoint

1. Login to your Azure Portal
1. Open the App Service `as-AzureMasterclass-dev` and navigate to `Networking` tab
1. In the `Outbound Traffic` section, click `VNet integration`
1. Click `Add VNet`, this will open a new dialog `Add VNet Integration`
   - Subscription: `Visual Studio Professional Subscription`
   - Virtual Network: `azure-masterclass-vnet`
   - Subnet: tick `Select Existing` and select `masterclass-appservice-outbound-subnet` from dropdown
   - Click `OK` and wait until the VNet Integration is ready
1. Navigate back to the `Networking` tab in the App Service, check the current status:
   - Inbound Traffic:
     - Access restriction: On
     - Private endpoints: On
   - Outbound Traffic:
     - VNet integration: On

## Test our Web App from the VM again

Now it's time to test our App Service from our VM on private network to make sure everything works as expected.

1. Login to your Azure Portal
1. Open the VM `azure-masterclass-vm-dev`
1. Navigate to `Bastion` tab, enter user name `azuremasterclasstester` and password
1. Click `Connect`
1. Wait for Windows 10 to finish initialization for the first time
1. Open Microsoft Edge and navigate to https://as-azuremasterclass-dev.azurewebsites.net

Our web app should still work as expected.
