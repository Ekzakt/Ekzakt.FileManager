using Azure.Identity;
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

public class AzureBlobFileManager : IFileManager
{
    private readonly ILogger<AzureBlobFileManager> _logger;
    private readonly AzureFileManagerOptions? _options;

    private BlobServiceClient? _blobServiceClient;
    private BlobContainerClient? _blobContainerClient;
    private SaveFileRequest _saveFileRequest;

    public AzureBlobFileManager(
        ILogger<AzureBlobFileManager> logger,
        IOptions<AzureFileManagerOptions> options)
    {
        _logger = logger;
        _options = options?.Value;
        _saveFileRequest = new();
    }



    public async Task<SaveFileResponse> SaveAsync(SaveFileRequest saveFileRequest, CancellationToken cancellationToken = default)
    {
        EnsureSaveFileRequest(saveFileRequest);
        EnsureBlobContainerClient(saveFileRequest.ContainerName);

        BlobClient blobClient = _blobContainerClient!.GetBlobClient(saveFileRequest.FileName);

        try
        {
            var blobResult = await blobClient.UploadAsync(saveFileRequest.InputStream, GetBlobUploadOptions(), cancellationToken);

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

            if (saveFileRequest.ThrowOnError) throw;

            return new SaveFileResponse { HttpStatusCode = HttpStatusCode.InternalServerError };

        }
        catch (Exception ex)
        {
            _logger.LogError("Error while saving file: {0}", ex);

            if (saveFileRequest.ThrowOnError) throw;

            return new SaveFileResponse { HttpStatusCode = HttpStatusCode.InternalServerError };
        }
    }




    #region Helpers


    private void EnsureBlobContainerClient(string containerName)
    {
        if (_blobContainerClient is not null && _blobContainerClient.Name.Equals(containerName, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        EnsureBlobServiceClient();

        _logger.LogInformation("Getting blob container {0}.", containerName);

        _blobContainerClient = _blobServiceClient?.GetBlobContainerClient(containerName);

        _logger.LogInformation("Successfully accessed blob container {0}.", containerName);
    }


    private void EnsureBlobServiceClient()
    {

        if (_blobServiceClient is null)
        {
            _logger.LogInformation("Connecting to Azure storage account.");

            _blobServiceClient = new BlobServiceClient(
               serviceUri: new Uri($"https://{_options?.Azure.StorageAccount.Name}.blob.core.windows.net"),
               credential: new DefaultAzureCredential());

            _logger.LogInformation("Connected successfully to Azure storage account.");
        }
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