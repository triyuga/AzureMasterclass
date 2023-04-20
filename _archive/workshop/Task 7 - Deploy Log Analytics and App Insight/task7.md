# Task 7 - Log analytics

It's recommended to use workspace-based Application insights.
- Support full integration between Application Insights and Log Analytics.
- Send Application Insights telemetry to a common Log Analytics workspace.
- Allow you to access the latest features of Azure Monitor while keeping application, infrastructure, and platform logs in a consolidated location.
- Enable common Azure role-based access control across your resources.
- Eliminate the need for cross-app/workspace queries.
- Are available in all commercial regions and Azure US Government.
- Don't require changing instrumentation keys after migration from a classic resource.


## Create Log analytics and App Insight from Azure

- Creation of App insight from Azure is straightforward. Notice the two are coupled in the creation process.
  - The output is a connection string on the overview page of app insight 

- For Log analytics, no code change required 
- For app insights,
  - `nuget install Microsoft.ApplicationInsights.AspNetCore`
    
    ```csharp
    builder.Services.AddApplicationInsightsTelemetry();
    ```
    ```json
    "ApplicationInsights": {
      "ConnectionString": "Copy connection string from Application Insights Resource Overview"
    }
    ```
- Update bicep 
  ```bicep
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
  ```


## Exercise:

### Deploy Log analytics and App Insight on DEV and TEST
- Update pipeline library for all groups 
- Enable for both API and Azure Function App



