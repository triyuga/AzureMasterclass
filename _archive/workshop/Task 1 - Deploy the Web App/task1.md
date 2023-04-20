# Task 2 - Deploy the Web App

Let's deploy the first version of our Web App to Azure environment!

Use projects in [src-stage1](../../src-stage1). Neither the web app nor SPA in this stage has any external dependencies (including AAD) which is ideal when you start to deploy the initial resources to Azure. This is also a very common strategy that we use in most customer engagements.

Some general suggestions:

- Start with a simple project without any external dependency
- Deploy the initial solution as early as possible because when your solution evolves with more complicity and dependencies involved, it becomes harder to get all the deployment steps correct at the first time
- Prefer small, incremental changes in the ARM template to massive changes because troubleshooting deployment steps is usually harder than debugging code on your local machine

## Azure resources to be deployed

For our projects in this stage to work properly on Azure, this is the minimum resources we need to deploy:

- Resource Group: A logical container of resources. Resources can be grouped and managed in different ways using resource group, but a common practice is to use resource group as environment boundary. We will also use this strategy in this masterclass.
  - [App Service](https://learn.microsoft.com/en-us/azure/app-service/overview)
    - the host of our Web API
    - Our React SPA is also hosted in the App Service as static resource
  - [App Service Plan](https://learn.microsoft.com/en-us/azure/app-service/overview-hosting-plans)
    - The underlying computing resource the App Service runs on
    - You can define OS type, region, VM instance number and size. They all contribute and relate directly to the overall price you pay for your Azure usage
    - Briefly, the higher spec you choose in the App Service plan, the higher your Azure subscription bill will be

## Azure Resource Naming Convention

Similar to naming convention in programming language, a good Azure resource naming convention will help the whole team in working with Azure resources in a more organized way.

We should follow our client's Azure Resource naming convention when working on client engagements.

In this masterclass, we will follow the [resource naming convention](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/resource-naming) defined in Microsoft's CAF standard.

## Different tools that we can use to deploy resources on Azure

There are various ways to deploy Azure resources.

- [Azure Portal](https://portal.azure.com)

  - A web-based interface designed for managing Azure Resources
  - You can create resource directly in Azure resource group by choosing from Azure Marketplace
  - A very user-friendly way to create resource for the first time because of its resource creation wizard with data validation and deployment preview
  - You can download the ARM templates right before the 'Create' button.
    - You may also export the ARM template after it's been created. More on that at the [Create App Service via template](#Create-App-Service-via-template) section below.

- [Azure CLI](https://learn.microsoft.com/en-au/cli/azure/what-is-azure-cli)

  - A cross-platform tool for managing resources on Azure
  - Can be used to create resource manually or be assembled in scripts for automation
  - Uses imperative approach
  - You can use Azure CLI directly in Azure Portal, a.k.a [Cloud Shell](https://portal.azure.com/#cloudshell/)

- [Azure Powershell](https://learn.microsoft.com/en-au/powershell/azure/what-is-azure-powershell)

  - A set of Poweshell cmdlets (most of them work the same as Azure CLI equivalents but their syntax are designed to fit Powershell naturally)
  - Can be used to create resource manually or be assembled in scripts for automation
  - Uses imperative approach

- [Azure Resource Manager (ARM) Template](https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/overview)
  - A declarative way to define Azure Resource (a.k.a. Infrastructure As Code)
  - Ensure idempotent result from multiple deployments
  - Supports both JSON and Bicep language
  - Mostly used in automation

## Create Resource Group via Cloud Shell

1. Login to your Azure Portal
1. Click the `Cloud Shell` button on top navigation bar
1. Choose "Bash"
1. Run command `az group create -l australiaeast -n AzureMasterClass`
1. You can remove the new group with command `az group delete -n AzureMasterClass`

## Create App Service Plan via Azure Portal

If you don't have much experience in Azure resource provisioning, we recommend you to start with Azure Portal. With the straightforward resource creation wizard UI, you will have a clear understanding of all the customizations you can choose from.

1. Login to your Azure Portal
1. Open the resource group `AzureMasterClass` you created in previous step
1. Click `Create` button on the top action bar
1. Choose `App Service Plan` by using the search function
1. You will see a resource provisioning wizard on first page `Basics`:
   - The Subscription and Resource Group have been automatically filled in
   - Name: `plan-AzureMasterClass-dev`
   - Operation System: `Linux`
   - Region: `Australia East`
   - Pricing Tier: `Free F1`
   - Zone redundancy: `Disabled`
   - Skip the `Tags` page and go straight to `Review + Create` Page
   - If you click `Create` button then the App Service Plan will be provisioned. Instead, let's click `Download a template for automation` button
   - Click `Download` button on the top action bar and save the template package to your machine.
   - Click `Create` button and wait until the App Service Plan is provisioned successfully.

## Create App Service via Azure Portal

Similarly, let's familiarise ourselves by creating an App Service via Azure portal.

1. Login to your Azure Portal
1. Open the resource group `AzureMasterClass` you created in previous step
1. Click `Create` button on the top action bar
1. Choose `Web App` by using the search function
1. You will see a resource provisioning wizard on first page `Basics`:
   - The subscription and Resource Group have been automatically filled in
   - Name: `as-AzureMasterclass-dev`
   - Publish: `Code`
   - Runtime stack: `.NET 6 (LTS)`
   - Operation System: `Linux`
   - Region: `Australia East`
   - Linux Plan: `plan-AzureMasterClass-dev` (the one we created in previous step)
1. On second page `Deployment`:
   - Leave everything in default value
1. On third page `Networking`
   - Leave everything in default value
1. On fourth page `Monitoring`
   - Application Insights: `No` (we will enable this in later stage)
1. On fifth page `Tags`
   - Leave everything in default value
1. On last page `Review  + Create`
   1. Click `Download a template for automation` button
   1. Click `Download` button on the top action bar and save the template package to your machine.
   1. Click `Create` button and wait until the Web App is provisioned successfully

## Create App Service via template

You can get a template from
- MS official references
- Download one right before you click 'Create' during the Azure Portal resource creation wizards.
- Export a template from existing resource from Azure Portal, or from CLI.
  - `az group export --resource-group <resource-group-name> --resource-ids $storageAccountID1 $storageAccountID2`

If you export them from Azure portal, you can export from 
- Resource group or resource
- Saves from Deployment history

Export the template from a resource group or resource, when:

- You need to capture changes to the resources that were made after the original deployment.
- You want to select which resources are exported.
- The resources weren't created with a template.

Export the template from the history, when:

- You want an easy-to-reuse template.
- You don't need to include changes you made after the original deployment.

Most exported templates require some modifications before they can be used to deploy Azure resources.
- long resource names
- states introduced etc.

[doc: Choose the right export option](https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/export-template-portal#choose-the-right-export-option)

### Introduction to Bicep

- Getting started with bicep. Can mention ARM as well, but kind of verbose
- Might be worth giving an initial version of the bicep template they can deploy from command line with just the app service? Either from local or need to upload file to AzureCLI storage. It did integrate with the online VSCode thing, so could be possible

[Bicep language reference](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/file)

### Define our first Bicep template

Create `main.bicep` with the following content:

```bicep
@description('Location for all resources.')
param location string = resourceGroup().location

@description('A unique suffix to add to resource names that need to be globally unique.')
@maxLength(13)
param resourceNameSuffix string = uniqueString(resourceGroup().id)

param linuxFxVersion string = 'DOTNETCORE|6.0' // The runtime stack of web app

@description('The name of the project.')
param projectName string = 'AzureMasterclass'

// Define the names for resources.
var appServiceAppName = 'as-${projectName}-${resourceNameSuffix}'
var appServicePlanName = 'plan-${projectName}'
```

### Convert ARM JSON template to Bicep

Although you can only export template from Azure resource creation wizard in JSON format, Microsoft provides an online tool which helps to convert JSON template to equivalent Bicep format.

1. Open [Bicep Playground](https://aka.ms/bicepdemo)
1. Click `Decompile` button at the top navigation bar and choose the exported ARM template on your machine
1. The converted Bicep template will be displayed in the left pane

You can rename the resource `name_resource` based on the naming convention in our project.

## Other Bicep Tools

[Reference](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install)

- [VSCode Extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-bicep): Provide intellisense and code navigation feaure
- [Azure CLI](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/install#azure-cli): You can convert JSON template to Bicep using this tool

## Define the App Service Plan in Bicep

Add the following code in our `main.bicep` file:

```bicep
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: true
  }
  sku: {
    name: 'B1'
  }
  kind: 'linux'
}
```

## Define the Web App in Bicep

Add the following code in our `main.bicep` file:

```bicep
resource appServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: linuxFxVersion
    }
  }
}
```

## Using `parameters.json` file (optional)

- Both the pipeline task `AzureResourceManagerTemplateDeployment@3`, and the CLI accept a json file as the input parameters for the deployment alternatively. 
  - Parameter files make it easier to package parameter values for a specific environment. 

- Shown in the bicep folder `main.parameters.json` is an example of passing partially the inputs needed.


```json
{
  "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
  "contentVersion": "1.0.0.0",
  "parameters": {
    "projectName": {
      "value": "AzureMasterclass"
    },
    "linuxFxVersion": {
      "value": "DOTNETCORE|6.0"
    }
  }
}

```

For a full reference, check out [Parameters in ARM templates](https://learn.microsoft.com/en-us/azure/azure-resource-manager/templates/parameters)

## Execute the Bicep on Azure via Azure Cloud

[Link](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/deploy-cloud-shell?tabs=azure-cli)

3. Deploy bicep `az deployment group create --resource-group MasterclassTask2 --template-file environment.bicep`

## Execute the Bicep on Azure via Azure Portal

1. Login to your Azure Portal
1. Open the resource group `AzureMasterClass` you created in previous step
1. Click `Create` button on the top action bar
1. Choose `Template deployment (deploy using custom templates)` by using the search function
1. Click `Build your own template in the editor`
1. Click `Upload file` and choose the Bicep file from your computer, and click `Save` button at the bottom
1. In the `Basic` page, double check the subscription, resource group and region
1. Head to `Review + Create` page and click `Create` button

## Add Service Connection in Azure DevOps

TBD

## Add Arm template deployment step in pipeline

Now we've successfully deployed the App Service Plan and Web App on Azure via Bicep template. Let's add the deployment step in our DevOps pipeline so that these resources will be updated anytime we updates the Bicep template.

1. Add the bicep file in source control: `git add ./deploy/bicep/main.bicep`

1 Add a new stage in our pipeline for deployment on DEV environment:

```yaml
- stage: deploy_to_dev
  displayName: deploy to dev
  jobs:
    - job: Deploy
      displayName: "Deploy"
      steps:
        - task: DownloadBuildArtifacts@1
          displayName: "Download Build Artifacts"
          inputs:
            artifactName: "drop"
            downloadPath: $(Build.ArtifactStagingDirectory)

        - task: AzureResourceManagerTemplateDeployment@3
          inputs:
            deploymentScope: "Resource Group"
            azureResourceManagerConnection: "$(ArmConnection)"
            subscriptionId: "$(SubscriptionId)"
            action: "Create Or Update Resource Group"
            resourceGroupName: "MasterClassDev"
            location: "Australia East"
            templateLocation: "Linked artifact"
            csmFile: "$(Build.ArtifactStagingDirectory)/drop/bicep/main.bicep"
            deploymentMode: "Incremental"
            deploymentOutputs: "outputStorageVar"
```

## Add Web App deployment step in pipeline

Now every time we change the Bicep file, the Web App resource resource will be updated accordingly. But we haven't deploy the actual code of the web app yet so it's still empty (oops). Let's add the code deployment step in pipeline now:

```yaml
  - task: AzureRmWebAppDeployment@4
          displayName: "Deploy App Service"
          inputs:
            azureSubscription: "$(ArmConnection)"
            appType: "webAppLinux"
            WebAppName: $(appServiceAppName)
            packageForLinux: "$(Build.ArtifactStagingDirectory)/drop/Output/AzureMasterclass.Api.zip"
```

## Exercise

- Add a new Dev environment
- Deploy the web app

Source code: [src-stage1](../../src-stage1/)
