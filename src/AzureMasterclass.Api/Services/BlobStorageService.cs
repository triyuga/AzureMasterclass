using Azure.Storage.Blobs;

namespace AzureMasterclass.Api.Services;

public class BlobStorageService: IBlobStorageService
{
    private BlobServiceClient _blobServiceClient;
    private readonly string _containerName;
    
    public BlobStorageService(BlobServiceClient blobServiceClient, string containerName)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = containerName;
    }

    public async Task<bool> CheckIfContainerExistsAsync(CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);

        return (await containerClient.ExistsAsync(cancellationToken)).Value;
    }

    public async Task DeleteBlobAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public async Task<byte[]?> ReadBlobAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        if (!await blobClient.ExistsAsync(cancellationToken))
        {
            return null;
        }

        using var ms = new MemoryStream();
        await blobClient.DownloadToAsync(ms, cancellationToken);

        return ms.ToArray();
    }

    public async Task WriteBlobAsync(string blobName, byte[] file, CancellationToken cancellationToken)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(new MemoryStream(file), cancellationToken);
    }
}