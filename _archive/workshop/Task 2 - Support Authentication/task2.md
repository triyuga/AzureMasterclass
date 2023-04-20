# Task 2: Support Authentication

# Steps

## 1. Enable Authentication with AAD

1. Go to "AAD" in the Azure portal
2. Choose "App registrations" on the left navigation pane.
3. "+ New registration" on the top menu.
4. Give a name and choose 'Accounts in this organizational directory only (Default Directory only - Single tenant)'
5. "Register".

Now we have the outputs:

- Application (client) ID
- Directory (tenant) ID
- The ability to add Client secret/certificate
  - Only if your API accesses a downstream API would it need its own credentials

### Good to know:

Azure App Service supports [Easy Auth](https://learn.microsoft.com/en-us/azure/app-service/overview-authentication-authorization) which requires no code configuration but can only function as barrier to entry for your app, in which case all you need to do is enable it in App Service.

Because we need to access resources on behalf of the user, we go with MSAL integration rather than Easy Auth.

If our user audience is everyone - social, public, private and external identity providers, we go with AAD B2C.
However, here we demonstrate MS identity only for simplicity, we choose just AAD.

MSAL supports many [auth flow types](https://learn.microsoft.com/en-us/azure/active-directory/develop/msal-authentication-flows) each of which caters to a certain application type.

For SPA, MSAL.js 2.0+ with authorization code is recommended.

![image](https://learn.microsoft.com/en-us/azure/active-directory/fundamentals/media/authentication-patterns/oauth.png)

## 2. Returning the token via Redirect URI

1. Select the app registration you created earlier.
2. Under Manage, select Authentication > Add a platform.
3. Under Web applications, select the Single-page application tile.
4. Under Redirect URIs, enter a redirect URI. Do NOT select either checkbox under Implicit grant and hybrid flows.
5. Select Configure to finish adding the redirect URI.

### Good to know:

Per this project, enter the localhost:3000 for redirect redirectUri for local env. If you reuse dev environment for local development, you may enter another one by clicking '+'.

In the SPA .env file, we can use `/` for `REACT_APP_REDIRECT_URI` which should satisfy all environments.

## 3. Accessing resources representing the user via Scope

1. Define the scope by selecting "Expose an API" in the navigation pane in AAD
2. Select Set next to Application ID URI if you haven't yet configured one.
3. Select Add a scope and fill out the descriptions.
4. for Who can consent, choose Admin and users. (Roles are out of scope. Check the references at the bottom.)
5. State is enabled, then select 'Add scope'.

### Good to know:

The user will pass the scope with its request to the protected resource such as web API.
Your web API then performs the requested operation only if the access token it receives contains the scopes required for the operation.
The token returned will contain those scopes.

## 4. Correlate scopes to API permission

1. Go to "API permissions" in the navigation pane.
2. Choose Add a permission > My APIs.
3. Select the web API you registered.
4. Under Select permissions, expand the resource whose scopes you defined for your web API, and select the permissions the client app should have on behalf of the signed-in user.
5. Select Add permissions to complete the process.

## 5. Add code to SPA for MSAL2.0.

1. Follow [doc: Configure your SPA](https://learn.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-react#configure-your-javascript-spa)

## 6. Add code to the Web API for enalbing Auth.

1. DI
   ```csharp
   builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
       .AddMicrosoftIdentityWebApi(azureAdOptions);
   ```
2. Middleware
   ```csharp
   app.UseRouting();
   // snippet start
   app.UseAuthentication();
   app.UseAuthorization();
   // stop
   app.MapControllers();
   ```
3. appsettings.Development.json
   ```json
   "AzureAd": {
      "Instance": "https://login.microsoftonline.com/",
      "TenantId": "<your tenant id>",
      "ClientId": "<your application/client id>"
   },
   ```
4. Decorate the controller or actions that you want to secure with `[Authorize]`

## 7. CI/CD automation for environment variables related to AAD

1. For the SPA, if you use the provided `.env` file, since it's compile time substitution, also because our pipeline methodology aim to only build once (and tweak configs at deployment times), our only choice alludes to token replacement during CD.
   Check out [replace-tokens.yml](../../deploy/jobs/replace-tokens.yml) which uses [Replace tokens](https://marketplace.visualstudio.com/items?itemName=qetza.replacetokens).

Note that this is a 3rd party task and you must first install it from the pipeline marketplace which requires admin permission.

2. For the WebApi, conveniently we have the official task support `FileTransform`.
   Check out [deploy.yml](../../deploy/jobs/deploy.yml).

3. Notice that here we do both CI and CD in the yml pipeline, passing env specific variables from its corresponding pipeline Library to the pipeline's various templates.
   ```yml
   - stage: deploy_to_dev
     displayName: deploy to dev
     variables:
       - group: "Dev"
     jobs:
   ```

## 8. Pipeline Variables and Secrets

1. You can define secret variables common to all environments in the pipeline variables, such as ARM connection, subscriptionId.
2. You can define Azure environment specific variables in Pipeline's Library. You can refer them in the pipeline directly. Note that you need to grant permission to where the library will be used. Choose the Permission from the menu.
3. Notice the naming where '.' is applied. This helped bring the json path for appsettings.json and the SPA token replacement together.

### Good to know:

KeyVault is also an viable option to both the pipelines and the bicep. Check out the references at the bottom.
Using KeyVault has the benefits of separation of concern, centralised secret management and RBAC.

## Exercise: Deploy the Web App with Authentication

1. Follow the above steps to finish app registration in Azure portal.
2. Regarding the SPA implementation, have a read on the snippets to understand how it works.
3. Use the output to test if AAD works on the local environment.
4. Think about how the environment variable replacement should be done to cater to multiple Azure environments.
   Try put them in the pipeline's Library and test.

Source code: [src-stage2](../../src-stage2/)

#### References:

[doc: AAD to register](https://learn.microsoft.com/en-us/azure/active-directory/develop/scenario-spa-app-registration#create-the-app-registration)

[doc: Expose web APIs](https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-expose-web-apis)

[doc: API permissions](https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-configure-app-access-web-apis)

[doc: Add app roles to your app](https://learn.microsoft.com/en-us/azure/active-directory/develop/howto-add-app-roles-in-azure-ad-apps)

[doc: Use Azure Key Vault secrets in Azure Pipelines](https://learn.microsoft.com/en-us/azure/devops/pipelines/release/azure-key-vault?view=azure-devops&tabs=yaml)

[doc: Set and retrieve a secret from Azure Key Vault using Bicep](https://learn.microsoft.com/en-us/azure/key-vault/secrets/quick-create-bicep?tabs=CLI)
