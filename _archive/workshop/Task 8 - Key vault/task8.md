# Task 8 - Key Vault

Instead of putting secrets in the pipeline (Library, pipeline variables, release variables), you can put all in Key Vault which is a standalone azure resource with RBAC. 
It also comes with a bunch of utilities such as activation / expiration dates, auto rotation etc.


## Concept
- Management Plane - about the key vault resource itself
- Data Plane - about the secrets / certs 

[doc: access model](https://learn.microsoft.com/en-us/azure/key-vault/general/security-features#access-model-overview)

## Create Key Vault from Azure Portal

- [doc: Create a vault](https://learn.microsoft.com/en-us/azure/key-vault/general/quick-create-portal#create-a-vault).
  - Before creation, download the template.
  - Outputs : name and key vault URI

- [doc: Set and retrieve a secret from Azure portal](https://learn.microsoft.com/en-us/azure/key-vault/secrets/quick-create-portal)


## Access Key vault from pipeline (yml)
- You can link secrets from KeyVault as variables by toggling the switch in Pipeline Library. Warning: It will erase the existing variables in this group.
  - [doc: Link secrets from an Azure key vault](https://learn.microsoft.com/en-us/azure/devops/pipelines/library/variable-groups?view=azure-devops&tabs=yaml#link-secrets-from-an-azure-key-vault)
    - If 'authorize' fails with 405 when linking, make sure you've granted at least GET and LIST to DevOps from the KeyVault's Access Policy.
      - Go to Access Policy and click '+ Create'
      - Select GET and LIST in the Secret Permission
      - Choose the principle representing your DevOps. 
      - Create
    - Select the secrets that you wish to link.
- Or, you may access Key Vault directly from the yml pipeline via [AzureKeyVault@2](https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/azure-key-vault-v2?view=azure-pipelines)
  - You still need to make sure the access policy is in place otherwise you'll get a 'forbidden' error.
  - Usage example:
  ```yml
  steps:
  - task: AzureKeyVault@2
    inputs:
      azureSubscription: 'Your-Azure-Subscription'
      KeyVaultName: 'Your-Key-Vault-Name'
      SecretsFilter: '*'
      RunAsPreJob: false

  - task: CmdLine@2
    inputs:
      script: 'echo $(Your-Secret-Name) > secret.txt'

  - task: CopyFiles@2
    inputs:
      Contents: secret.txt
      targetFolder: '$(Build.ArtifactStagingDirectory)'

  - task: PublishBuildArtifacts@1
    inputs:
      PathtoPublish: '$(Build.ArtifactStagingDirectory)'
      ArtifactName: 'drop'
      publishLocation: 'Container'
  ```

## Access Key vault from ARM (bicep)

- Or, you have the option of accessing Key Vault from bicep via the [getSecret](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/key-vault-parameter?tabs=azure-cli#use-getsecret-function) function.
```bicep
param sqlServerName string
param adminLogin string

param subscriptionId string
param kvResourceGroup string
param kvName string

resource kv 'Microsoft.KeyVault/vaults@2019-09-01' existing = {
  name: kvName
  scope: resourceGroup(subscriptionId, kvResourceGroup )
}

module sql './sql.bicep' = {
  name: 'deploySQL'
  params: {
    sqlServerName: sqlServerName
    adminLogin: adminLogin
    adminPassword: kv.getSecret('vmAdminPassword') // access by the function
  }
}
```
- Or, access from the parameter file
  - [doc: reference from the parameter file](https://learn.microsoft.com/en-us/azure/azure-resource-manager/bicep/key-vault-parameter?tabs=azure-cli#reference-secrets-in-parameter-file)
  ```json
  {
    "$schema": "https://schema.management.azure.com/schemas/2019-04-01/deploymentParameters.json#",
    "contentVersion": "1.0.0.0",
    "parameters": {
        "adminLogin": {
        "value": "exampleadmin"
        },
        "adminPassword": {
            "reference": {// <-- accessed here
                "keyVault": {
                    "id": "/subscriptions/<subscription-id>/resourceGroups/<rg-name>/providers/Microsoft.KeyVault/vaults/<vault-name>"
                },
                "secretName": "ExamplePassword"
            }
        },
        "sqlServerName": {
        "value": "<your-server-name>"
        }
    }
    }
  ```

## Access key vault from App Configuration

- Or, if you use App Configuration (not covered in this master class), you can access from there.
  - [doc: Add a Key Vault reference to App Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/use-key-vault-references-dotnet-core?tabs=core5x#add-a-key-vault-reference-to-app-configuration)

## Access Key vault from code (.NET)



## ARM for Key Vault

If we decide to use Key Vault, we should automate the deployment using bicep like this.

```bicep
param name string
param location string
param sku string
param accessPolicies array
param tenant string
param enabledForDeployment bool
param enabledForTemplateDeployment bool
param enabledForDiskEncryption bool
param enableRbacAuthorization bool
param publicNetworkAccess string
param enableSoftDelete bool
param softDeleteRetentionInDays int
param networkAcls object

resource keyVault 'Microsoft.KeyVault/vaults@2021-10-01' = {
  name: name
  location: location
  properties: {
    enabledForDeployment: enabledForDeployment
    enabledForTemplateDeployment: enabledForTemplateDeployment
    enabledForDiskEncryption: enabledForDiskEncryption
    enableRbacAuthorization: enableRbacAuthorization
    accessPolicies: accessPolicies
    tenantId: tenant
    sku: {
      name: sku
      family: 'A'
    }
    publicNetworkAccess: publicNetworkAccess
    enableSoftDelete: enableSoftDelete
    softDeleteRetentionInDays: softDeleteRetentionInDays
    networkAcls: networkAcls
  }
  tags: {
  }
  dependsOn: []
}
```

## Exercise

For this exercise, let's just try out key vault with one secret migrated from the pipeline.

1. Create a Key Vault resource from Azure Portal.
2. Set a secret to be migrated from azure pipeline. E.g. a connection string.
3. Choose a method from above. E.g. Link the secret from the Library.
4. Access it from the corresponding place.
5. Remove the original pipeline variable and test.

