using System.IO.Compression;
using Azure.Storage.Blobs;
using blobstorage.Exceptions;

namespace blobstorage.Services;

public class AzureBlobStorageService : IAzureBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient) =>
        _blobServiceClient = blobServiceClient;

    /// <inheritdoc cref="IAzureBlobService.DeleteBlobAsync"/>
    public async Task<bool> DeleteBlobAsync(string blobName,
        string containerName, CancellationToken cancellationToken)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName)
            ?? throw new BlobContainerNotFoundException();

        return (await blobContainerClient.DeleteBlobIfExistsAsync(blobName,
            cancellationToken: cancellationToken)).Value;
    }

    /// <inheritdoc cref="IAzureBlobService.DownloadBlobAsync"/>
    public async Task<Stream> DownloadBlobAsync(string blobName,
        string containerName, CancellationToken cancellationToken)
    {
        var blobClient = await GetBlobClientAsync(blobName, containerName)
            ?? throw new BlobClientNotFoundException();

        using var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    /// <inheritdoc cref="IAzureBlobService.DownloadAndCompressBlobsAsync"/>
    public async Task<MemoryStream> DownloadAndCompressBlobsAsync(List<string> blobNames,
        string containerName, CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();

        using var zipArchive = new ZipArchive(memoryStream,
            ZipArchiveMode.Create, leaveOpen: true);
        foreach (var blobName in blobNames)
        {
            using Stream entry = zipArchive.CreateEntry(blobName,
                CompressionLevel.Optimal).Open();
            var blobClient = await GetBlobClientAsync(blobName, containerName);
            await blobClient.DownloadToAsync(entry, cancellationToken);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }

    /// <inheritdoc cref="IAzureBlobService.GetBlobClientAsync"/>
    public Task<BlobClient> GetBlobClientAsync(string blobName, string containerName)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName)
            ?? throw new BlobContainerNotFoundException();

        var blobClient = blobContainerClient.GetBlobClient(blobName)
            ?? throw new BlobClientNotFoundException();

        return Task.FromResult(blobClient);
    }

    /// <inheritdoc cref="IAzureBlobService.SaveBlobAsync"/>
    public async Task<string> SaveBlobAsync(IFormFile blob, string blobName,
        string containerName, CancellationToken cancellationToken)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = blobContainerClient.GetBlobClient(blobName);

        if (await blobClient.ExistsAsync(cancellationToken))
        {
            throw new BlobAlreadyExistsException();
        }

        await blobClient.UploadAsync(blob.OpenReadStream(), cancellationToken);

        return blobClient.Uri.AbsolutePath;
    }
}

public interface IAzureBlobStorageService
{
    /// <summary>
    /// Delete blob from Azure Blob Storage
    /// </summary>
    /// <param name="blobName"></param>
    /// <param name="containerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BlobContainerNotFoundException"></exception>
    Task<bool> DeleteBlobAsync(string blobName,
        string containerName, CancellationToken cancellationToken);

    /// <summary>
    /// Download blob from Azure Blob Storage as a stream
    /// </summary>
    /// <param name="blobName"></param>
    /// <param name="containerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BlobClientNotFoundException"></exception>
    Task<Stream> DownloadBlobAsync(string blobName,
        string containerName, CancellationToken cancellationToken);

    /// <summary>
    /// Download mulitple blobs from Azure Blob Storage and compress
    /// into a single Zip file
    /// </summary>
    /// <param name="blobNames"></param>
    /// <param name="containerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MemoryStream> DownloadAndCompressBlobsAsync(List<string> blobNames,
        string containerName, CancellationToken cancellationToken);

    /// <summary>
    /// Get the Blob Client from Azure Blob Storage
    /// </summary>
    /// <param name="blobName"></param>
    /// <param name="containerName"></param>
    /// <returns></returns>
    /// <exception cref="BlobContainerNotFoundException"></exception>
    /// <exception cref="BlobClientNotFoundException"></exception>
    Task<BlobClient> GetBlobClientAsync(string blobName, string containerName);

    /// <summary>
    /// Save file to Azure Blob Storage
    /// </summary>
    /// <param name="blob"></param>
    /// <param name="blobName"></param>
    /// <param name="containerName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BlobAlreadyExistsException"></exception>
    Task<string> SaveBlobAsync(IFormFile blob, string blobName,
        string containerName, CancellationToken cancellationToken);
}