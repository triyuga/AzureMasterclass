# Task 5 - Deploy Azure Storage Accounts

## Understand key concepts if you're interested Azure Exams

- Types of accounts / services / redundancy / usage
- Types of blob storages / usage
- Types of access authorisation / usage

## Local development setup

The 'scripts' folder contains docker-compose files for both Windows and macOS which uses Azurite for storage account simulation.

Run the setup-development-environment.ps1 which runs the docker-compose command.

`pwsh setup-development-environment.ps1 -reset` will force reset docker container and set up local db and storage emulation, and run DbUp.

Have a read of the script to find out if you need to run the whole script or partially.

## Storage Account connection string

- Locally, for Azurite, we use a well known connection string for the emulator as listed [here](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio#http-connection-strings)

  - There's also a shortcut version [here](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio#connection-strings)

- For Azure environment, there are the following options depending on your means of authorisation.
  - (Traditional) Connection string: navigate to Security + networking > Access keys in your storage account's settings to see connection strings for both primary and secondary access keys.
  - [Other means of connection](https://learn.microsoft.com/en-us/azure/storage/common/storage-configure-connection-string#create-a-connection-string-using-a-shared-access-signature) which are out of this scope.
  - Azure AD / Managed identity
    - It's non-secret.
    - the value is simply the url for your blob service. Check it out in Settings -> Endpoints -> Blob service

## Test out the local dev

With Azurite and its connection string in place, we can implement related functionality locally now.

- Put the connection string and the container name in `appsettings.Development.json`

```json
  "ConnectionStrings": {
    "BlobStorage": "DefaultEndpointsProtocol=https;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
  }
  "Blob": {
    "ContainerName": "storage0"
  }

```

- You can read them using the option pattern

```csharp
var blobOptions = builder.Configuration.GetSection(BlobOptions.Blob).Get<BlobOptions>();
var connectionStringOptions = builder.Configuration.GetSection(ConnectionStringOptions.ConnectionStrings).Get<ConnectionStringOptions>();
```

- Factory for the required client and our own service as a wrapper
  (requiring nuget `Microsoft.Extensions.Azure`, `Azure.Storage.Blobs` and `Azure.Identity`)

```csharp
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddClient<BlobServiceClient, BlobClientOptions>(
        (options, _, __) =>
            IsDevelopment
                ? new BlobServiceClient(connectionStringOptions.BlobStorage, options)
                : new BlobServiceClient(new Uri(connectionStringOptions.BlobStorage), new DefaultAzureCredential(), options));
});

```

> `DefaultAzureCredential` supports multiple authentication methods and determines the authentication method being used at runtime. In this way, your app can use different authentication methods in different environments without implementing environment specific code.

With the wrapper `BlobStorageService` and existing code for the book cover feature in place, you can run the app locally (exercise 1).

## Create a Storage Account in Azure

Creating a Storage Account in Azure is pretty straight forward. For all the options to choose, you can refer [doc: Create a storage account in Azure](https://learn.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal) for more contexts.

Let's create it in Azure Portal manually to get familiar with all the options.

1. Login to your Azure Portal
1. Open the resource group `AzureMasterClass`
1. Click `Create` button on the top action bar
1. Choose `Storage Account` by using the search function
1. You will see a resource provisioning wizard on first page `Basics`:
   - The Subscription and Resource Group have been automatically filled in
   - Storage Account name: `samasterclassdev`
   - Region: `Australia East`
   - Performance: `Standard`
   - Redundancy: `Locally-redundant storage(LRS)`
1. Leave `Advanced`, `Networking`, `Data protection`, `Encryption` and `Tags` pages untouched
1. In `Review + create` page:
   - Click `Download a template for automation` button
   - Click `Download` button on the top action bar and save the template package to your machine
   - Click `Create` button and wait until the App Service Plan is provisioned successfully

Don't forget to 'Download a template for automation' right before creation.

> Note
> When you create an Azure Storage account, you are not automatically assigned permissions to access data via Azure AD. You must explicitly assign yourself an Azure role for access to Blob Storage. You can assign it at the level of your subscription, resource group, storage account, or container.

## Enable managed identity and grant roles

1. Go to the storage account you created -> IAM
2. Choose '+ Add' -> Add role assignment
3. Choose 'Storage Blb Data Contributor' for the Role. You can filter to find it.
4. Next, choose 'Managed Identity' as the 'assigned access to'.
5. Click '+ Select members', then on the right popover pane, use the filters to file your app service (or which ever resource you want assign the role to)
6. Omit Conditions as it's out of scope, 'Review and assign'

## Test the storage account in azure

You can try to connect to the resource you created in Azure with the local environment

- You may need to add your IP address to the while list depending on your Network configuration.
  - Go to Security + networking -> Networking for this config.
- Temporarily replace the connection string for `ConnectionStrings.BlobStorage` with the one from managed identity.
- Run the app.

## Update Bicep

```bicep
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

// Blob storage containers in bicep helps other resources' need for storage.
// But you can also programmatically create containers if there's no dependencies with other resources.
resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = [for i in range(0, 2): {
  name: '${storageAccount.name}/default/storage${i}'
}]

```

## Update DevOps pipeline

- Go to Pipeline -> Library, for each variable group create an entry for the blob storage connection string in line with the appsettings.json's path such as `ConnectionStrings.BlobStorage`.

Make sure these library variables are replaced at CD times.

- You may use `FileTransform@1` task in the pipeline to do variable injection.

```yml
- task: FileTransform@1
  inputs:
    folderPath: "${{parameters.path}}/Output/$(funcAppName).zip"
    fileType: "json"
    targetFiles: "appsettings.json"
```

## Exercise: Deploy Azure Storage Account to both DEV and TEST

1. Set up and run the app locally.
2. Deploy the app to azure and check its function there.

Source code: [src-stage4](../../src-stage4/)
