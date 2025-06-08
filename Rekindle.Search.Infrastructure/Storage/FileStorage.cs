using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Rekindle.Search.Application.Storage.Interfaces;
using Rekindle.Search.Application.Storage.Models;

namespace Rekindle.Search.Infrastructure.Storage;

public class FileStorage : IFileStorage
{
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "memories";

    public FileStorage(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task<Guid> UploadAsync(Stream stream, string contentType,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobName = Guid.NewGuid();
        var blobClient = containerClient.GetBlobClient(blobName.ToString());

        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType },
            cancellationToken: cancellationToken);

        return blobName;
    }

    public async Task<FileResponse> GetAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        var blobDownloadInfo = await blobClient.DownloadAsync(cancellationToken);

        return new FileResponse(fileId, blobDownloadInfo.Value.Content, blobDownloadInfo.Value.Details.ContentType);
    }

    public async Task DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        var blobClient = containerClient.GetBlobClient(fileId.ToString());

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}