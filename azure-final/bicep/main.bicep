@description('Describes plan\'s pricing tier and instance size. Check details at https://azure.microsoft.com/en-us/pricing/details/app-service/')
@allowed([
  'F1'
  'D1'
  'B1'
  'B2'
  'B3'
  'S1'
  'S2'
  'S3'
  'P1'
  'P2'
  'P3'
  'P4'
])

param skuName string = 'F1'

@description('Select the type of environment you want to provision. Allowed values are Production and Test.')
@allowed([
  'Prod'
  'Test'
  'Dev'
])
param environmentType string

@description('Location for all resources.')
param location string = resourceGroup().location

@description('A unique suffix to add to resource names that need to be globally unique.')
@maxLength(13)
param resourceNameSuffix string = uniqueString(resourceGroup().id)

param linuxFxVersion string //= 'DOTNETCORE|6.0' // The runtime stack of web app

@description('The name of the project.')
param projectName string // = 'AzureMasterclass'

@description('The administrator login username for the SQL server.')
param sqlServerAdministratorLogin string

@secure()
@description('The administrator login password for the SQL server.')
param sqlServerAdministratorLoginPassword string

@description('Azure AD Instance')
param azureAdInstance string

@description('Azure AD Tenant ID')
param azureAdTenantId string

@description('Specifies client id for Azure AD.')
param clientIdForAzureAd string

// Define the names for resources.
var environmentAbbreviation = environmentConfigurationMap[environmentType].environmentAbbreviation
// var keyVaultName = 'kv-${projectName}-${environmentAbbreviation}'
var appServiceAppName = 'as-${projectName}-${resourceNameSuffix}-${environmentAbbreviation}'
var appServicePlanName = 'plan-${projectName}-${environmentAbbreviation}'
var logAnalyticsWorkspaceName = 'log-${projectName}-${environmentAbbreviation}'
var applicationInsightsName = 'appi-${projectName}-${environmentAbbreviation}'
var sqlServerName = 'sql-${projectName}-${resourceNameSuffix}-${environmentAbbreviation}'
var sqlDatabaseName = '${projectName}-${environmentAbbreviation}'
var storageAccountName = 'sa${resourceNameSuffix}${toLower(environmentAbbreviation)}'
//var blobStorageName = 'blob-${projectName}-${resourceNameSuffix}-${environmentAbbreviation}'
//var messageQueueName = 'queue-${projectName}-${resourceNameSuffix}-${environmentAbbreviation}'
var functionAppName = 'f${projectName}-${resourceNameSuffix}-${environmentAbbreviation}'

// Per environment variable configurations
var environmentConfigurationMap = {
  Prod: {
    environmentAbbreviation: 'prod'
    appServicePlan: {
      sku: {
        name: 'S1'
        capacity: 1
      }
    }
    storageAccount: {
      sku: {
        name: 'Standard_LRS'
      }
    }
    sqlDatabase: {
      sku: {
        name: 'Standard'
        tier: 'Standard'
      }
    }
  }
  Test: {
    environmentAbbreviation: 'test'
    appServicePlan: {
      sku: {
        name: 'B1'
      }
    }
    storageAccount: {
      sku: {
        name: 'Standard_LRS'
      }
    }
    sqlDatabase: {
      sku: {
        name: 'Basic'
      }
    }
  }
  Dev: {
    environmentAbbreviation: 'dev'
    appServicePlan: {
      sku: {
        name: 'B1'
      }
    }
    storageAccount: {
      sku: {
        name: 'Standard_LRS'
      }
    }
    sqlDatabase: {
      sku: {
        name: 'Basic'
      }
    }
  }
}

// Log analytics
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

// Application insights
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

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

// App service plan for app service
resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  properties: {
    reserved: true
  }
  sku: environmentConfigurationMap[environmentType].appServicePlan.sku
  kind: 'linux'
}

// App service to run API and website
resource appServiceApp 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      linuxFxVersion: linuxFxVersion
    }
  }
}

// resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
//   name: serviceBusNamespaceName
//   location: location
//   sku: {
//     name: 'Standard'
//   }
//   properties: {}
// }

// resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-01-01-preview' = {
//   parent: serviceBusNamespace
//   name: serviceBusQueueName
//   properties: {
//     lockDuration: 'PT5M'
//     maxSizeInMegabytes: 1024
//     requiresDuplicateDetection: false
//     requiresSession: false
//     defaultMessageTimeToLive: 'P10675199DT2H48M5.4775807S'
//     deadLetteringOnMessageExpiration: false
//     duplicateDetectionHistoryTimeWindow: 'PT10M'
//     maxDeliveryCount: 10
//     autoDeleteOnIdle: 'P10675199DT2H48M5.4775807S'
//     enablePartitioning: false
//     enableExpress: false
//   }
// }

// Storage account for hosting blob storage
resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: environmentConfigurationMap[environmentType].storageAccount.sku
  properties: {
    accessTier: 'Hot'
  }
}

// Blob storage containers
resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = [for i in range(0, 2): {
  name: '${storageAccount.name}/default/storage${i}'
}]

// Inject configuration into app service
resource webSiteConnectionStrings 'Microsoft.Web/sites/config@2022-03-01' = {
  parent: appServiceApp
  name: 'connectionstrings'
  properties: {
    DbConnection: {
      //value: 'Data Source=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${databaseName};User Id=${sqlAdministratorLogin}@${sqlServer.properties.fullyQualifiedDomainName};Password=${sqlAdministratorLoginPassword};'
      value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlServerAdministratorLogin};Password=${sqlServerAdministratorLoginPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
      type: 'SQLAzure'
    }
  }
}

resource webSiteAppAuthenticationSettings 'Microsoft.Web/sites/config@2021-03-01' = {
  name: 'authsettingsV2'
  kind: 'string'
  parent: appServiceApp
  properties: {
    globalValidation: {
      requireAuthentication: false
    }
    httpSettings: {
      requireHttps: true
    }
    platform: {
      enabled: false
    }
    identityProviders: {
      azureActiveDirectory: {
        enabled: true
        registration: {
          openIdIssuer: '${azureAdInstance}${azureAdTenantId}'
          clientId: clientIdForAzureAd
          clientSecretSettingName: 'AzureAd:ClientSecret'
        }
      }
    }
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
          value: '~4'
        }
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '~14'
        }
        {
          name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
          value: applicationInsights.properties.InstrumentationKey
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

output appServiceAppName string = appServiceApp.name
output appServiceAppHostName string = appServiceApp.properties.defaultHostName
output sqlServerName string = sqlServer.properties.fullyQualifiedDomainName
output functionAppName string = functionApp.name
