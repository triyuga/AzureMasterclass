# Task 3 - Deploy to a new environment

By the time we deploy to a replicate of the initial dev environment, we should already have a working CI/CD pipeline including an ARM/bicep template. 

The goal in this task is to dissecting the steps and modularise them.

There are difference approaches for replicating builds and environments, each with pros and cons. Perhaps more apparent initially is to use Azure DevOps's Pipeline for CI and Release for CD. the Release feature is based on UI and isn't part of the VCS, which makes a big con of it.

Empirically we prefer putting everything in VCS, also 'build once and deploy the artefacts multiple times per environment' provides cleaner and drier process.

That's exactly what we adopted in this project.

So the Dev pipeline will build and deploy to dev env; the Test pipeline will deploy to test env using the dev artefacts.

- The build stage is not reused.
- The deploy / DbUp stage will be reused

## Add a new resource group in Azure

Assuming we have a basic bicep file in the pipeline form Task 1, here we only need to parameterise the environment related parts, and refactor to reuse the ARM deployment task so a new environment can be provisioned automatically.

### Key concepts

1. Basics: variables, parameters, expressions.

> In a compile-time expression (${{ <expression> }}), you have access to parameters and statically defined variables. 
> In a runtime expression ($[ <expression> ]), you have access to more variables but no parameters. It's designed for use with conditions and expressions.
> Macro syntax variables ($(var)) get processed during runtime before a task runs. Macro syntax is designed to interpolate variable values into task inputs and into other variables. Macro syntax variables are only expanded for stages, jobs, and steps. You cannot, for example, use macro syntax inside a resource or trigger.

Read more on

- [variable](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/variables?view=azure-devops&tabs=yaml%2Cbatch#understand-variable-syntax)
- [expression](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/expressions?view=azure-devops)

2. Basics: templates

You can define templates in separate yml file and 'include' or 'extend' them like

```yml
- jobs:
    - job: Deploy
      displayName: "Deploy"
      steps:
        - download: current
          artifact: drop

        - template: jobs/deploy.yml # <- here. inclusion of codes at compile time.
          parameters:
            envName: Dev
            path: $(Pipeline.Workspace)/drop
```

and the template included defines parameters like this

```yml
parameters:
  - name: envName
    displayName: Environment name
    values:
      - Dev
      - Test
      - Prod
  - name: path
    displayName: Path for the downloaded artefact
    type: string

steps:
```

Extension can provide advanced control for security, and is out of scope of this demo.

Read more on

- [template](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/templates?view=azure-devops)
- [security through templates](https://learn.microsoft.com/en-us/azure/devops/pipelines/security/templates?view=azure-devops)

## Add a new App Registration

Following Task 2 to register for the Test app.

## Add a new pipeline and variable group in Azure DevOps

- Put the environment variables for Test in Library.
- Create a new pipeline for the test app from DevOps Pipeline.
  - Choose Pipelines on the left navigation pane
  - Click top right the button - New Pipeline
  - Choose Azure repos Git -> {the name of your repo} -> Existing Azure Pipelines YAML file
  - On the popover to the right, choose the branch (master), then give the path to the yml file.

## Structure changes in DevOps pipeline

Refactor to so the pipeline reuse deploy.

- The test pipeline can look like

```yml
stages:
  - stage: deploy_to_test
    displayName: deploy to test
    variables:
      - group: "Test"
    jobs:
      - job: Deploy
        displayName: "Deploy"
        pool:
          vmImage: "ubuntu-latest"
        steps:
          - download: DevArtifact
            artifact: drop

          - template: jobs/deploy.yml # <- reuse
            parameters:
              envName: Test
              path: $(Pipeline.Workspace)/DevArtifact/drop

      - job: DbUp
        dependsOn: Deploy
        steps:
          - template: jobs/dbup.yml # <- reuse
```

For the deploy.yml,

```yml
parameters:
  - name: envName
    displayName: Environment name
    values:
      - Dev
      - Test
      - Prod
  - name: path
    displayName: Path for the downloaded artefact
    type: string

steps:
  - task: AzureResourceManagerTemplateDeployment@3
    inputs:
      # deploymentScope: "Resource Group" # 'Management Group' | 'Subscription' | 'Resource Group'. Required. Deployment scope. Default: Resource Group.
      azureResourceManagerConnection: "$(ArmConnection)"
      subscriptionId: "$(SubscriptionId)"
      # action: "Create Or Update Resource Group" # 'Create Or Update Resource Group' | 'DeleteRG'. Required when deploymentScope = Resource Group. Action. Default: Create Or Update Resource Group.
      resourceGroupName: "MasterClass${{parameters.envName}}"
      location: "Australia East"
      # templateLocation: "Linked artifact" # 'Linked artifact' | 'URL of the file'. Required. Template location. Default: Linked artifact.
      csmFile: "${{parameters.path}}/bicep/main.bicep"
      overrideParameters: "-sqlServerAdministratorLogin $(sqlServerAdministratorLogin) -sqlServerAdministratorLoginPassword $(sqlServerAdministratorLoginPassword) -environmentType ${{parameters.envName}} -azureAdTenantId $(azureAdTenantId) -azureAdInstance $(AzureAd.Instance) -clientIdForAzureAd $(clientIdForAzureAd)"
      # deploymentMode: "Incremental"
      deploymentOutputs: "outputStorageVar"

  - task: PowerShell@2
    inputs:
      targetType: "inline"
      script: |
        $obj = ConvertFrom-Json '$(outputStorageVar)'
        Write-Host "##vso[task.setvariable variable=appServiceAppName]$($obj.appServiceAppName.value)"
        Write-Host "##vso[task.setvariable variable=appServiceAppHostName]$($obj.appServiceAppHostName.value)"
        Write-Host "##vso[task.setvariable variable=sqlServerName]$($obj.sqlServerName.value)"
        Write-Host "##vso[task.setvariable variable=functionAppName]$($obj.functionAppName.value)"

  - template: ./replace-tokens.yml
    parameters:
      zipPath: "${{parameters.path}}/Output/$(webApiName).zip"

  - task: FileTransform@1
    inputs:
      folderPath: "${{parameters.path}}/Output/$(webApiName).zip"
      fileType: "json"
      targetFiles: "appsettings.json"

  - task: AzureWebApp@1
    inputs:
      azureSubscription: "$(ArmConnection)"
      appType: "webAppLinux"
      appName: "$(appServiceAppName)"
      package: "${{parameters.path}}/Output/$(webApiName).zip"
```

For dbup reuse,

```yml
steps:
  - task: UseDotNet@2
    inputs:
      packageType: "sdk"
      version: $(dotnetSdkVersion)

  - task: FileTransform@1
    inputs:
      folderPath: "./src/$(dbUpName)"
      fileType: "json"
      targetFiles: "appsettings.json"

  - task: DotNetCoreCLI@2
    displayName: "Run database migrations"
    inputs:
      command: "run"
      projects: "./src/$(dbUpName)"
```

## Exercise: Deploy the Web App to TEST environment

1. Make a copy of the default pipeline you made for dev, and find out what varies between environments.
   1. In the current folder there's a file azure-pipelines.yml for reference.
2. Test it out by creating a Test pipeline in Azure Portal and see everything deploys successfully if you substitute the vars for Test.
3. Refactor the pipeline so that it
   1. Download the artefact from Dev pipeline
   2. Deploy with the Test vars.

Source code: [src-stage2](../../src-stage2/)

## Troubleshooting

1. If you find your deploy fails after taking a long time, it could be that you need to scape up your app service.
   ref: https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/azure-function-app-v1?view=azure-pipelines#a-release-hangs-for-long-time-and-then-fails
