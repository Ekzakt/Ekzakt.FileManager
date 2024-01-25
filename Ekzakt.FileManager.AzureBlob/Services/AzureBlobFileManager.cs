using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage;
using Azure;
using Ekzakt.FileManager.AzureBlob.Configuration;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ekzakt.FileManager.AzureBlob.Services;
using System.Net;
using Ekzakt.FileManager.Core.Validators;
using FluentValidation;
using System.Text.RegularExpressions;

public class AzureBlobFileManager : IFileManager
{
    private readonly ILogger<AzureBlobFileManager> _logger;
    private readonly FileManagerOptions? _options;
    private readonly BlobServiceClient? _blobServiceClient;

    private BlobContainerClient? _blobContainerClient;
    private SaveFileRequest _saveFileRequest;


    public AzureBlobFileManager(
        ILogger<AzureBlobFileManager> logger,
        IOptions<FileManagerOptions> options,
        BlobServiceClient blobServiceClient,
        IValidator<FileManagerOptions> validator)
    {
        validator.ValidateAndThrow(options.Value);

        _logger = logger;
        _blobServiceClient = blobServiceClient;
        _options = options?.Value;
        _saveFileRequest = new();
    }



    public async Task<SaveFileResponse> SaveAsync(SaveFileRequest saveFileRequest, CancellationToken cancellationToken = default)
    {
        try
        {
            EnsureBlobContainerClient(saveFileRequest.ContainerName);
            EnsureSaveFileRequest(saveFileRequest);

            var blobClient = _blobContainerClient!.GetBlobClient(saveFileRequest.FileName);
            var blobResult = await blobClient.UploadAsync(saveFileRequest.InputStream, GetBlobUploadOptions(), cancellationToken);
            
            _logger.LogInformation("File {0} successfully created in blobcontainer {1}. RawResponse: {2}", saveFileRequest.FileName, saveFileRequest.ContainerName, blobResult.GetRawResponse());

            var output = new SaveFileResponse
            {
                FileName = _saveFileRequest.FileName,
                Message = $"File {_saveFileRequest.FileName} successfully saved.",
                HttpStatusCode = HttpStatusCode.Created
            };

            return output;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogError("Container not found: {0}", saveFileRequest.ContainerName);

            return new SaveFileResponse
            {
                FileName = _saveFileRequest.FileName,
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }
        catch (ValidationException ex)
        {
            _logger.LogError("One or more validation errors occured: {0}", ex);

            var message = ex.Errors.FirstOrDefault()?.ToString();
            
            if (message != null)
            {
                message = Regex.Unescape(message);
            }

            return new SaveFileResponse
            {
                FileName = _saveFileRequest.FileName,
                Message = $"One or more validation errors occured: {message}",
                HttpStatusCode = HttpStatusCode.BadRequest
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while saving file: {0}", ex);

            return new SaveFileResponse
            {
                FileName = _saveFileRequest.FileName,
                Message = $"Error while saving file: {ex.Message}",
                HttpStatusCode = HttpStatusCode.InternalServerError
            };
        }
    }




    #region Helpers


    private void EnsureBlobContainerClient(string containerName)
    {
        if (_blobContainerClient is not null && _blobContainerClient.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _logger.LogInformation("Getting blob container {0}.", containerName);

        _blobContainerClient = _blobServiceClient?.GetBlobContainerClient(containerName);

        _logger.LogInformation("Successfully accessed blob container {0}.", containerName);
    }


    private void EnsureSaveFileRequest(SaveFileRequest saveFileRequest)
    {
        var validator = new SaveFileRequestValidator();

        validator.ValidateAndThrow(saveFileRequest);

        _saveFileRequest = saveFileRequest;
    }


    private BlobUploadOptions GetBlobUploadOptions()
    {
        var blobOptions = new BlobUploadOptions
        {
            ProgressHandler = new FileProgressHandler(_saveFileRequest),
            TransferOptions = new StorageTransferOptions
            {
                MaximumConcurrency = Environment.ProcessorCount * 2,
                MaximumTransferSize = 50 * 1024 * 1024
            }
        };

        return blobOptions;
    }


    #endregion Helpers

}