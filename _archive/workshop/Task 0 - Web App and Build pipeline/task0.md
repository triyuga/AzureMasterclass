# Web App and Build pipeline

## Goal of this task

The goal of this task is to get familiar with the sample projects and Azure DevOps build pipeline which we will use (and extend) in the upcoming tasks.

These two projects were designed with these principles in mind:

- Only does one job => easy to understand
- Has minimum external dependencies (packages, etc) => avoid distraction

In order to focus on the Azure resource deployment instead of spending a lot of time adding new features in our web app, we've prepared multiple stages of projects to be used throughout the masterclass. Here's a quick summary of them:

Stage 1

- A basic Web API
- An React SPA
  How to build and deploy them through Azure CI/CD pipeline yaml.

Stage 2

- Ability to login/logoff
  How to integrate Azure AD and manage identities.

Stage 3

- Ability to list/add/update/delete books in SPA
- The API connects to SQL database as
  How to integrate Azure SQL for data persistence.

Stage 4:

- Ability to add/update book's cover image
  How to integrate Azure Storage Account for blob storage.

Stage 5:

- Ability to look up ISBN numbers in background
  How to integrate Azure Function as a serverless solution.

## Prerequisite

- Has a personal Azure subscription (from your Telstra Purple MSDN account)
- Know how to login on Azure Portal
- Establish a service connection from DevOps to Azure (for things like web site deployment)
  - Follow [doc: Create a Service Connection](https://learn.microsoft.com/en-us/azure/devops/pipelines/library/service-endpoints?view=azure-devops&tabs=yaml#create-a-service-connection)

## Azure DevOps Build pipeline

We use Azure DevOps pipeline to automate the build, test and deployment process. There are two variants of Azure DevOps pipelines that you can use:

- Classic Pipeline: Build and test jobs are defined in YAML while deployment jobs are defined in a dedicated UI in DevOps, as described [here](https://learn.microsoft.com/en-us/azure/devops/pipelines/release/define-multistage-release-process?view=azure-devops). One of the major drawback is not be able to source control and reuse defined release tasks. Although Microsoft does not have a plan to deprecate the product, we do not recommend use the Classic Pipeline in new projects because of the reason above.

- Multi-stage Pipeline: Build, test and deployment are defined in YAML files, which is the solution Microsoft promotes and new features are added to it constantly. You can find the official document [here](https://learn.microsoft.com/en-us/azure/devops/pipelines/get-started/key-pipelines-concepts?view=azure-devops).

In this masterclass we will use multi-stage pipeline exclusively.

### Key components in pipeline

- Trigger: what triggers the pipeline to run, e.g. new commits in a specific branch, a new build artifact, etc.
- Agent: Virtual machines which run pipeline jobs. They can be Microsoft-hosted or user-hosted.
- Logical hierarchy: Stage -> Job -> Step. All the steps in the same job are executed on the same agent.

To keep this masterclass brief and focused, we've prepared the build stage in pipeline.

[link to pipeline yaml in stage 0]()

### Key features in pipeline

#### Dependency

By default, pipeline stages, jobs and steps run in the top-down order; but this can be changed by specifying dependencies explicitly. Here are [some examples](https://learn.microsoft.com/en-us/azure/devops/pipelines/process/stages?view=azure-devops&tabs=yaml#specify-dependencies)

This feature makes it possible to trigger deployment to multiple environments after build stage succeeds.

#### Condition

By default, pipeline stages and jobs will continue running until there's any error. However, we can change the behavior by adding customized conditions in order to achieve these controls:

- Skip a job in certain condition (e.g. skip the deployment stages on PR builds)
- Pause the pipeline (e.g. wait for manual approval before deploying to Production)

### How to edit the pipeline

It's just a YAML file so any text editor which supports YAML format should be sufficient.

You can also use the Azure DevOps built-in pipeline editor. Here are some additional features in it:

- Pipeline-specific intellisense and error checking
- Ability to add new task definition via UI

Check [this page](https://learn.microsoft.com/en-us/azure/devops/pipelines/get-started/yaml-pipeline-editor?view=azure-devops) for more details about the editor.

### Artifact folder structure

You can inspect the published

- drop
  - bicep
    - main.bicep
  - Output
    - AzureMasterclass.Api.zip

## Exercise

- Create and push to your own repo
- Add the build pipeline and finish the first build

Source code: [src-stage1](../../src-stage1/)
