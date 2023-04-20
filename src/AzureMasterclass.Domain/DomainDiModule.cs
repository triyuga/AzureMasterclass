using Azure.Identity;
using AzureMasterclass.Domain.Services;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AzureMasterclass.Domain;

public static class DomainDiModule
{
    private static bool IsDevelopment =>
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?.ToLower() == "development"
        || Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")
            ?.ToLower() == "development";
    
    public static void RegisterDomain(IServiceCollection services, IConfiguration configuration, EnabledFeatures enabledFeatures)
    {
        if (enabledFeatures.Sql) {
            services.AddScoped<IIsbnSearchService, IsbnSearchService>();
            services.AddDbContext<IAzureMasterclassDatabaseContext, AzureMasterclassDatabaseContext>(
                (_, dbContextBuilder) =>
                {
                    var connection = new SqlConnection();
                    connection.ConnectionString = configuration.GetConnectionString("Masterclass");
            
                    if (!IsDevelopment)
                    {
                        // retrieve the access token from Azure using managed identity
                        // official support for managed identity will be added in EF Core 7
                        // ref: https://github.com/dotnet/efcore/issues/13261
                        var credential = new DefaultAzureCredential();
                        var token = credential.GetToken(
                            new Azure.Core.TokenRequestContext(
                                new[] {"https://database.windows.net/.default"}));
                        connection.AccessToken = token.Token;
                    }
                    else
                    {
                        dbContextBuilder.EnableSensitiveDataLogging();
                    }
                    dbContextBuilder.UseSqlServer(connection);
                });
        }
    }
}