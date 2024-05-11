using Azure;
using Azure.Storage.Blobs;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Models.Responses;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Operations;

public abstract class AbstractAzureFileOperation<TLogger> : AbstractFileOperation<TLogger>
    where TLogger : class
{
    private readonly ILogger<TLogger> _logger;
    private readonly BlobServiceClient _blobServiceClient;

    private BlobContainerClient? _blobContainerClient;

    public BlobServiceClient BlobServiceClient => _blobServiceClient;

    public BlobContainerClient? BlobContainerClient => _blobContainerClient;


    public AbstractAzureFileOperation(
        ILogger<TLogger> logger,
        BlobServiceClient blobServiceClient) : base(logger)
    {
        _logger = logger;
        _blobServiceClient = blobServiceClient;
    }


    public bool EnsureBlobContainerClient<TResponse>(string containerName, out FileResponse<TResponse?> response)
        where TResponse : class?
    {
        if (_blobContainerClient is not null && _blobContainerClient.Name.Equals(containerName))
        {
            response = new();

            return true;
        }
        try
        {
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            response = new();

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogError("Blobcontainer {BlobContainerName} does not exist. Exception: {Exception}", containerName, ex);

            response = new FileResponse<TResponse?>
            {
                Status = HttpStatusCode.NotFound,
                Message = "Blob container not found."
            };

            return false;
        }
    }
}
