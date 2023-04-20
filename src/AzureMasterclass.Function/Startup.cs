using System;
using AzureMasterclass.Domain;
using AzureMasterclass.Domain.Options;
using AzureMasterclass.Function;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Startup))]

namespace AzureMasterclass.Function;

public class Startup : FunctionsStartup
{
    public IConfiguration Configuration { get; private set; }

    public override void Configure(IFunctionsHostBuilder builder)
    {   
        var azureAdOptions = Configuration.GetSection(AzureAdOptions.AzureAd).Get<AzureAdOptions>();
        var connectionStringOptions = Configuration.GetSection(ConnectionStringOptions.ConnectionStrings).Get<ConnectionStringOptions>();
        var blobStorageOptions = Configuration.GetSection(BlobStorageOptions.Blob).Get<BlobStorageOptions>();
        
        var enabledFeatures = new EnabledFeatures(azureAdOptions, connectionStringOptions, blobStorageOptions);
        DomainDiModule.RegisterDomain(builder.Services, Configuration, enabledFeatures);
    }
    
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        var context = builder.GetContext();
        var config = builder.ConfigurationBuilder
            .SetBasePath(context.ApplicationRootPath)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{context.EnvironmentName}.json", optional: true)
            .AddJsonFile($"appsettings.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        Configuration = config;
    }
    
}