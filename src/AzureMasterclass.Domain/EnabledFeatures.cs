using AzureMasterclass.Domain.Options;

namespace AzureMasterclass.Domain;

public class EnabledFeatures
{
    public bool Auth { get; set; } = false;
    public bool Sql { get; set; } = false;
    public bool BlobStorage { get; set; } = false;

    public EnabledFeatures () {
        Auth = false;
        Sql = false;
        BlobStorage = false;
    }

    public EnabledFeatures (AzureAdOptions azureAdOptions, ConnectionStringOptions connectionStringOptions, BlobStorageOptions blobStorageOptions) {
        var isAuthEnabled = 
            !String.IsNullOrEmpty(azureAdOptions.Instance) 
            && !String.IsNullOrEmpty(azureAdOptions.TenantId)
            && !String.IsNullOrEmpty(azureAdOptions.ClientId);

        var isSqlEnabled = !String.IsNullOrEmpty(connectionStringOptions.Masterclass);
        
        var isStorageEnabled = 
            !String.IsNullOrEmpty(blobStorageOptions.ContainerName)
            && !String.IsNullOrEmpty(blobStorageOptions.ConnectionString);

        Auth = isAuthEnabled;
        Sql = isAuthEnabled && isSqlEnabled;
        BlobStorage = isAuthEnabled && isStorageEnabled;
    }
}