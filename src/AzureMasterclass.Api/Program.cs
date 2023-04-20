using Azure.Identity;
using Azure.Storage.Blobs;
using AzureMasterclass.Api.Services;
using AzureMasterclass.Domain;
using AzureMasterclass.Domain.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using SwaggerFilter.Filters;

var builder = WebApplication.CreateBuilder(args);

bool isDevelopment = 
    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLower() == "development"
    || Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")?.ToLower() == "development";

var connectionStringOptions = builder.Configuration.GetSection(ConnectionStringOptions.ConnectionStrings).Get<ConnectionStringOptions>();
var blobStorageOptions = builder.Configuration.GetSection(BlobStorageOptions.Blob).Get<BlobStorageOptions>();
var azureAdOptions = builder.Configuration.GetSection(AzureAdOptions.AzureAd).Get<AzureAdOptions>();

var enabledFeatures = new EnabledFeatures(azureAdOptions, connectionStringOptions, blobStorageOptions);

DomainDiModule.RegisterDomain(builder.Services, builder.Configuration, enabledFeatures);
builder.Services.AddSingleton<EnabledFeatures>(enabledFeatures);

builder.Services.AddControllers();

builder.Services.AddCors();

if (enabledFeatures.Auth) {
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration);

    builder.Services.AddApplicationInsightsTelemetry();
}

if (enabledFeatures.BlobStorage) {
    builder.Services.AddAzureClients(clientBuilder =>
    {
        // Add a KeyVault client
        // clientBuilder.AddSecretClient(keyVaultUrl);
        clientBuilder.AddClient<BlobServiceClient, BlobClientOptions>(
            (options, _, __) =>
                isDevelopment
                    ? new BlobServiceClient(blobStorageOptions.ConnectionString, options)
                    : new BlobServiceClient(new Uri(blobStorageOptions.ConnectionString), new DefaultAzureCredential(), options));
    });
    builder.Services.AddSingleton<IBlobStorageService>(sp => new BlobStorageService(sp.GetRequiredService<BlobServiceClient>(), blobStorageOptions.ContainerName));
}

builder.Services.AddSwaggerGen(options =>
{
    options.DocumentFilter<SwaggerDocumentFilter>();
    options.CustomOperationIds(description => $"{description.ActionDescriptor.RouteValues["controller"]}_{description.HttpMethod}");
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Weather and Books",
        Description = "azure master class",
    });
});

var app = builder.Build();

if (isDevelopment) {
    app.UseCors(builder => builder
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins("http://localhost:3000", "http://localhost:5000"));
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}
else {
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

if (enabledFeatures.Auth) {
    app.UseAuthentication();
}

app.UseAuthorization();

// Public endpoints
app.MapControllerRoute(
    name: "weather",
    pattern: "{controller=WeatherForecast}"
);
app.MapControllerRoute(
    name: "enabledFeatures",
    pattern: "{controller=EnabledFeatures}"
);


// Authenticated Endpoints
if (enabledFeatures.Auth) {
    app.MapControllerRoute(
        name: "user",
        pattern: "{controller=User}"
    );
}

// SQL dependent endpoints
if (enabledFeatures.Sql) {
    app.MapControllerRoute(
        name: "books",
        pattern: "{controller=Books}"
    );
    app.MapControllerRoute(
        name: "authors",
        pattern: "{controller=Authors}"
    );
}

app.MapFallbackToFile("index.html");

app.Run();
