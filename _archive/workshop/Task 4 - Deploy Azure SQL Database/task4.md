# Task 4 - Deploy Azure SQL database

## New features in Web App

Now let's add some "real" features which utilizes the SQL database into our web app.

New features/changes we added in project stage 3:

- Ability to list/add/update/delete books
- Local dev env:
  - API connects to local SQL server instance for data persistence
  - DbUp connects to local SQL server instance for database migration
  - A SQL server container is added in docker-compose file

[Link to stage 3 project code](TBD)

Please run your API and SPA with the corresponding docker containers on your local machine to make sure that the new features work before the Azure deployment.

## Add Azure SQL Instance in Bicep

Firstly, Let's add Azure SQL in our resource group:

```bicep
@description('The administrator login username for the SQL server.')
param sqlServerAdministratorLogin string

@secure()
@description('The administrator login password for the SQL server.')
param sqlServerAdministratorLoginPassword string

var sqlServerName = 'sql-${projectName}-${resourceNameSuffix}-${environmentAbbreviation}'
var sqlDatabaseName = '${projectName}-${environmentAbbreviation}'

// SQL server
resource sqlServer 'Microsoft.Sql/servers@2021-11-01' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlServerAdministratorLogin
    administratorLoginPassword: sqlServerAdministratorLoginPassword
    version: '12.0'
  }
}

// SQL Database
resource sqlDatabase 'Microsoft.Sql/servers/databases@2021-11-01' = {
  name: sqlDatabaseName
  parent: sqlServer
  location: location
  sku: environmentConfigurationMap[environmentType].sqlDatabase.sku
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 1073741824
  }
}

// Database firewall restrictions
resource allowAllWindowsAzureIps 'Microsoft.Sql/servers/firewallRules@2021-11-01' = {
  parent: sqlServer
  name: 'AllowAllWindowsAzureIps'
  properties: {
    endIpAddress: '0.0.0.0'
    startIpAddress: '0.0.0.0'
  }
}

```

## Define Database Admin Credential in DevOps Variable Group:

They need to be defined in both `Dev` and `Test` variable groups.

| Property                            | PROD                                                                                                                                                                                                                                                                           |
| ----------------------------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ConnectionStrings.Masterclass       | Server=tcp:$(sqlServer),1433;Initial Catalog=$(sqlDatabase);Persist Security Info=False;User ID=$(sqlServerAdministratorLogin);Password=$(sqlServerAdministratorLoginPassword);MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30; |
| sqlServer                           | sql-azuremasterclass-dev.database.windows.net                                                                                                                                                                                                                                  |
| sqlDatabase                         | AzureMasterclass-dev                                                                                                                                                                                                                                                           |
| sqlServerAdministratorLogin         | notadmin                                                                                                                                                                                                                                                                       |
| sqlServerAdministratorLoginPassword | Pa55w0rd!                                                                                                                                                                                                                                                                      |

## Inject Database Admin Credential in DevOps pipeline:

Please note that we defined the database credential as template parameters which need to be injected from our DevOps pipeline:

```yaml
- stage: deploy_to_dev
  displayName: deploy to dev
  variables:
    - group: "Dev" # <== here
  #......
  csmFile: "$(Build.ArtifactStagingDirectory)/drop/bicep/main.bicep"
  overrideParameters: "-sqlServerAdministratorLogin $(sqlServerAdministratorLogin)" # <== here
```

## Replace token in appsettings.json

Our API uses the connection string in `appsettings.json` to connect to Azure SQL database. After deployment, the connection string needs to be replaced with the correct value in each environment otherwise the SQL authentication will fail.

Specify the `appsettings.json` file in which the connection string needs to be replaced with environment variable:

```yaml
- task: AzureWebApp@1
  inputs:
    azureSubscription: "$(ArmConnection)"
    appType: "webAppLinux"
    appName: "$(appServiceAppName)"
    package: "${{parameters.path}}/Output/$(webApiName).zip"
```

Azure will replace those json properties whose path match the environment variable name.

For instance, if our `appsettings.json` file contains the following properties:

```json
{
  "Australia": {
    "Brisbane": "sunshine"
  }
}
```

If we define an environment variable as:

```bash
Australia.Brisbane="rainbow"
```

Then after variable substitution, the `appsettings.json` will become:

```json
{
  "Australia": {
    "Brisbane": "rainbow"
  }
}
```

Now our API can connect to database properly on Azure, but if you try to add a new Book on UI, you will see an error. That's because the database and its schema hasn't been created yet.

## Add DbUp

### What is DbUp

[DbUp](https://dbup.readthedocs.io/en/latest/) is a .NET tool which helps us to manage and deploy database changes. In a nutshell:

- Scripts are executed in alphabetical order
- One script will only be executed once (by maintaining a MigrationHistory table)
- Can be used in most database platforms
- Can be used to deploy both table schema and seeding data

### Different ways to run DbUp

There are several typical patterns of running DbUp script on Azure environment. Some settings are a bit complicated and can be covered in a separate masterclass for more details. So we are trying to keep the summary as brief as possible here:

Solution 1:

- Add DbUp in API project
- Run DbUp every time API starts

Solution 2:

- Add DbUp as a separate endpoint in API project
- Call the endpoint after deployment

Solution 3:

- Build DbUp as a separate project
- Run DbUp from DevOps release pipeline from Microsoft-hosted agents

Solution 4:

- Build DbUp as a separate project
- Provision an Azure VM and enroll it as a self-hosted DevOps agent
- Run DbUp from DevOps release pipeline from the self-hosted agent

For simplicity we will use solution 3 in this masterclass.

### Allow database connection from Microsoft-hosted DevOps agents

1. Login to your Azure Portal
1. Navigate to SQL Server `sql-AzureMasterclass-dev`
1. Open page `Networking`
1. Under `Exceptions`, tick checkbox `Allow Azure services and resources to access this server`

### Add DbUp task in pipeline

Add a new task at the bottom of `azure-pipelines.yml` so that DbUP can be executed AFTER the deployment of our web app:

```yaml
- job: DbUp
  dependsOn: Deploy
  steps:
    - task: UseDotNet@2
      inputs:
        packageType: "sdk"
        version: "$(dotnetSdkVersion)"
    - task: FileTransform@1
      inputs:
        folderPath: "./src/AzureMasterclass.DbUp"
        fileType: "json"
        targetFiles: "appsettings.json"
    - task: DotNetCoreCLI@2
      displayName: "Run database migrations"
      inputs:
        command: "run"
        projects: "./src/AzureMasterclass.DbUp"
```

## Introduction: SQL credential vs managed identity

So far our API authenticates against SQL Server using SQL Admin credential. It works but it's far from ideal because we still need to:

- Add the SQL credential as pipeline secrets (on each environment)
- Update credentials when they expire
- The credential needs to be injected in `appsettings.json` so it increases the risk of password leaking

Our recommended solution for these is using [managed identity](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/overview).

By enabling managed identity:

- We no longer need to manage database credential anymore
- Database connection string no longer contain password; it can be in plain text makes troubleshooting a lot easier

Instruction on how to enable managed identity to access Azure Sql: https://learn.microsoft.com/en-us/azure/app-service/tutorial-connect-msi-sql-database?tabs=cli%2Cef%2Cdotnet

Rahul wrote a very comprehensive blog post which explains how to enable managed identity for Azure SQL: https://www.rahulpnath.com/blog/azure-sql-server-managed-identity/

The project code in this stage has already been updated to use managed identity for database connection on Azure environment.

## Enable managed identity on Web API

Let's enable the managed identity in our Bicep template:

```bicep
resource appServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceAppName
  location: location
  identity:{
    type: 'SystemAssigned'  // <= here
  }
}
```

## Enable your IP Address in Azure SQL

1. Login to your Azure Portal
1. Navigate to SQL Server `sql-AzureMasterclass-dev`
1. Open page `Networking`
1. Under `Firewall rules`, click button `Add your client IPv4 address (xxx.xxx.xxx.xxx)`

## Assign your Azure AD account as Azure SQL Admin

1. Login to your Azure Portal
1. Navigate to SQL Server `sql-AzureMasterclass-dev`
1. Open page `Azure Active Directory`
1. Click button `Set Admin` on the top action bar and assign your Azure account

## Add managed identity as database user

1. Open SSMS (or Azure Data Studio) on your machine
1. Login to our Azure SQL Server using your Azure AD account (the one you assigned in previous step)
1. Choose database `AzureMasterclass-dev`
1. Execute the following commands to add managed identity as database user and assign permissions:

```sql
CREATE USER [as-AzureMasterclass-dev] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER as-AzureMasterclass-dev;
ALTER ROLE db_datawriter ADD MEMBER as-AzureMasterclass-dev;
GO
```

## Replace database connection string

Now we can remove SQL credentials from our connection string by updating our environment variable:

| Name                          | Value                                                |
| ----------------------------- | :--------------------------------------------------- |
| ConnectionStrings.Masterclass | Server=tcp:$(sqlServer),1433;Database=$(sqlDatabase) |

## Exercise

- Deploy Azure SQL instance
- Update the web app with database connection

Source code: [src-stage3](../../src-stage3/)
