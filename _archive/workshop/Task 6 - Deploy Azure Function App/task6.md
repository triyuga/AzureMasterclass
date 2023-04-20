# Task 6 - Deploy Azure Function App

In terms of deployment steps, Azure Function App is very similar to App Service; but there are some noticeable differences between them.

[Link to stage 4 project code](TBD)

## Features in the Function App

Check out our project code from [stage 4]() which contains a new Azure Function project in the solution.

- It's timer-triggered
- It iterates through all the books which don't have ISBN number associated and query [Google Book API](https://developers.google.com/books/docs/v1/reference/volumes/list) to get the corresponding ISBN numbers.

Please note that the whole solution has been slightly refactored by moving some common features such as entities and database context setup to a separate `AzureMasterclass.Domain` project so that they can be shared between Web API and Function App projects.

## Create Azure Function App in Azure Portal

Let's create the Function App manually first to get familiar with all the options.

1. Login to your Azure Portal
1. Open the resource group `AzureMasterClass`
1. Click `Create` button on the top action bar
1. Choose `Function App` by using the search function
1. You will see a resource provisioning wizard on first page `Basics`:
   - The Subscription and Resource Group have been automatically filled in
   - Function App name: `fAzureMasterclass-dev`
   - Publish: `Code`
   - Runtime Stack: `.NET`
   - Version: `6`
   - Region: `Australia East`
   - Operation System: `Linux`
   - Plan Type: `App Service Plan`
   - Linux Plan: `plan-AzureMasterclass-dev(B1)` (yes we are sharing the same one with App Service)
1. In the `Storage` page:
   - Storage Account: `samasterclassdev(v2)` (the one we created in previous step)
1. Leave `Networking`, `Monitoring`, `Deployment` and `Tags` pages untouched
1. In `Review + create` page:
   - Click `Download a template for automation` button
   - Click `Download` button on the top action bar and save the template package to your machine
   - Click `Create` button and wait until the App Service Plan is provisioned successfully

## Define Azure Function App and Azure Storage Account in Bicep

With the exported ARM template, we can add them to our Bicep template:

```bicep
var storageAccountName = 'sa${resourceNameSuffix}${toLower(environmentAbbreviation)}'
var functionAppName = 'f${projectName}-${resourceNameSuffix}-${environmentAbbreviation}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
  }
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~2'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~10'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet'
        }
      ]
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}

You might also notice that we also enabled managed identity in the Bicep template so that we can use it for database connection later.
```

## Add Function App deployment task in Pipeline

Now it's time to deploy the Function App executables to Azure in our DevOps pipeline. Don't forget the environment variable substitution step which replaces the connection string in `appsettings.json`.

```yaml
- task: FileTransform@1
  inputs:
    folderPath: "${{parameters.path}}/Output/$(funcAppName).zip"
    fileType: "json"
    targetFiles: "appsettings.json"

- task: AzureFunctionApp@1
  displayName: "Azure Function Deploy"
  inputs:
    azureSubscription: "$(ArmConnection)"
    appType: functionAppLinux
    appName: "$(functionAppName)"
    package: "${{parameters.path}}/Output/$(funcAppName).zip"
    startUpCommand: "func azure functionapp publish $(functionAppName) --no-bundler"
    runtimeStack: DOTNET|6.0
```

## Enable Managed Identity

Because the function App needs to connect to database for querying and updating `Book` table, the easiest way is still by allowing managed identity of the Function App in our Azure SQL Server.

1. Open SSMS (or Azure Data Studio) on your machine
1. Login to our Azure SQL Server using your Azure AD account (the one you assigned in previous step)
1. Choose database `AzureMasterclass-dev`
1. Execute the following commands to add managed identity as database user and assign permissions:

```sql
CREATE USER [fAzureMasterclass-dev] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER fAzureMasterclass-dev;
ALTER ROLE db_datawriter ADD MEMBER fAzureMasterclass-dev;
GO
```

## Update Function App Configurations

Application settings in a function app contain configuration options that we can customize which affects the entire scope of the function app.

In Azure Portal, you can view and edit the App Configuration in `Function App` -> `Deployment` -> `Configuration` -> `Application settings` tab.

Let's define the timer schedule in the App Configuration to demonstrate this feature.

In the Function App project you might have noticed that the timer schedule setting `SearchBookSchedule` has been defined in `local.settings.json` and referenced in `SearchBookFunction.Run()`.

Let's define a new variable in `DEV` environment to indicate the Function App to execute every 30 mins:

| Name               | Value             |
| ------------------ | :---------------- |
| SearchBookSchedule | \* \*/30 \* \* \* |

Feel free to define a different timer schedule on `TEST` environment.

Finally, we can inject the timer schedule value from environment variable into App Settings:

```yaml
- task: AzureFunctionApp@1
  displayName: "Azure Function Deploy"
  inputs:
    azureSubscription: "$(ArmConnection)"
    appType: functionAppLinux
    #......
    appSettings: "-SearchBookSchedule $(SearchBookSchedule)" # <== here
```

## Exercise: Deploy Azure Function App to both DEV and TEST

Source code: [src-stage5](../../src-stage5/)
