using Azure.Storage.Blobs;
using Azure;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Operations;

public abstract class AbstractFileOperation<TLogger>
    where TLogger : class
{
    private readonly ILogger<TLogger> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private BlobContainerClient? _blobContainerClient;

    public BlobServiceClient BlobServiceClient => _blobServiceClient;

    public BlobContainerClient? BlobContainerClient => _blobContainerClient;


    public AbstractFileOperation(
        ILogger<TLogger> logger,
        BlobServiceClient blobServiceClient)
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


    public bool ValidateRequest<TRequest, TValidator, TResponse>(TRequest request, TValidator validator, out FileResponse<TResponse?> response)
        where TRequest : AbstractFileRequest
        where TValidator : AbstractValidator<TRequest>
        where TResponse : class
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));

            _logger.LogWarning("{requestName} validation failed. Error: {errorMessage}",
                typeof(TRequest).Name,
                errorMessage
            );

            response = new FileResponse<TResponse?>
            {
                Status = HttpStatusCode.BadRequest,
                Message = errorMessage,
            };

            return false;
        }

        response = new();

        return true;
    }
}
