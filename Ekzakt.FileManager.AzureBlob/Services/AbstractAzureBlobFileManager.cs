using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Models;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Services;

public abstract class AbstractAzureBlobFileManager
{
    private readonly BlobServiceClient _blobServiceClient;


    public BlobContainerClient? BlobClient { get; private set; }
    public ILogger<AzureBlobFileManager> Logger { get; private set; }


    public AbstractAzureBlobFileManager(
        ILogger<AzureBlobFileManager> logger,
        BlobServiceClient blobServiceClient
        )
    {
        Logger = logger;
        _blobServiceClient = blobServiceClient;
    }


    public IFileManagerResponse EnsureBlobContainerClient(string containerName)
    {
        if (BlobClient is not null && BlobClient.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
        {
            return new FileResponse
            {
                StatusCode = HttpStatusCode.Accepted
            };
        }

        try
        {
            Logger.LogTrace("Getting blob container {0}.", containerName);

            BlobClient = _blobServiceClient?.GetBlobContainerClient(containerName);

            Logger.LogTrace("Successfully accessed blob container {0}.", containerName);

            return new FileResponse
            {
                StatusCode = HttpStatusCode.Accepted
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogError("BlobContainer {0} not found. {1}", containerName, ex);

            return new FileResponse
            {
                Message = "Blob container not found.",
                StatusCode = HttpStatusCode.NotFound
            };
        }
    }

    public IFileManagerResponse EnsureRequest<TRequest, TValidator>(TRequest request, TValidator validator)
        where TRequest : class
        where TValidator : IValidator<TRequest>
    {
        var result = validator.Validate(request);

        if (result is not null && result.IsValid) 
        {
            return new FileResponse { StatusCode = HttpStatusCode.Accepted };
        }

        return new FileResponse { StatusCode = HttpStatusCode.BadRequest };
    }
}
