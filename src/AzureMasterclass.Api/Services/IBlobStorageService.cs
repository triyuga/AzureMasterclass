namespace AzureMasterclass.Api.Services;

public interface IBlobStorageService
{
    Task<bool> CheckIfContainerExistsAsync(CancellationToken cancellationToken = default);

    Task DeleteBlobAsync(string blobName, CancellationToken cancellationToken = default);

    Task<byte[]?> ReadBlobAsync(string blobName, CancellationToken cancellationToken = default);

    Task WriteBlobAsync(string blobName, byte[] file, CancellationToken cancellationToken = default);
}