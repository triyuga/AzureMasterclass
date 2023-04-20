namespace AzureMasterclass.Domain.Options;

public class BlobStorageOptions
{
    public const string Blob = "BlobStorage";

    public string ContainerName { get; set; } = "";
    public string ConnectionString { get; set; } = "";
}