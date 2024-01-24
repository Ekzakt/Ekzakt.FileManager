using Azure.Identity;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Azure.Storage;
using Azure;
using Ekzakt.FileManager.AzureBlob.Configuration;
using Ekzakt.FileManager.AzureBlob.Models;
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

            var output = new SaveFileResponse {
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

namespace Ekzakt.FileManager.AzureBlob.Services;

{
    {


    }


    #endregion Helpers
}
