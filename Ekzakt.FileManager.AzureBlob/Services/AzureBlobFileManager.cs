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
using FluentValidation;
using System.Text.RegularExpressions;
using Ekzakt.FileManager.Core.Validators;
using Azure.Core;

public class AzureBlobFileManager : AbstractAzureBlobFileManager, IFileManager
{
    private readonly FileManagerOptions? _options;

    private SaveFileRequest _saveFileRequest;
    private IValidator<SaveFileRequest> _requestValidator;

    public AzureBlobFileManager(
        ILogger<AzureBlobFileManager> logger,
        IOptions<FileManagerOptions> options,
        IValidator<SaveFileRequest> requestValidator,
        BlobServiceClient blobServiceClient)
        : base(logger, blobServiceClient)
    {
        _options = options?.Value;
        _saveFileRequest = new();
        _requestValidator = requestValidator;
    }



    public async Task<IFileManagerResponse> SaveAsync(SaveFileRequest saveFileRequest, CancellationToken cancellationToken = default)
    {
        var blobRequest = EnsureBlobContainerClient(saveFileRequest.ContainerName);
        if (!blobRequest.IsSuccess)
        {
            return blobRequest;
        }

        var fileRequest = EnsureRequest(saveFileRequest, _requestValidator);
        if (!fileRequest.IsSuccess)
        {
            return fileRequest;
        }


        try
        {
            var blobClient = BlobClient!
                .GetBlobClient(saveFileRequest.FileName);

            var blobResult = await blobClient.UploadAsync(
                saveFileRequest.InputStream, 
                GetBlobUploadOptions(), 
                cancellationToken);
            
            Logger.LogTrace("File {0} successfully created in blobcontainer {1}. RawResponse: {2}", saveFileRequest.FileName, saveFileRequest.ContainerName, blobResult.GetRawResponse());

            var output = new SaveFileResponse(
                message: $"File {_saveFileRequest.FileName} successfully saved.",
                statusCode: HttpStatusCode.Created);

            return output;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogError("Container not found: {0}", saveFileRequest.ContainerName);

            return new SaveFileResponse{
                FileName = _saveFileRequest.FileName,
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
        catch (ValidationException ex)
        {
            Logger.LogError("One or more validation errors occured: {0}", ex);

            var message = ex.Errors.FirstOrDefault()?.ToString();
            
            if (message != null)
            {
                message = Regex.Unescape(message);
            }

            return new SaveFileResponse
            {
                FileName = _saveFileRequest.FileName,
                Message = $"One or more validation errors occured: {message}",
                StatusCode = HttpStatusCode.BadRequest
            };
        }
        catch (Exception ex)
        {
            Logger.LogError("Error while saving file: {0}", ex);

            return new SaveFileResponse
            {
                FileName = _saveFileRequest.FileName,
                Message = $"Error while saving file: {ex.Message}",
                StatusCode = HttpStatusCode.InternalServerError
            };
        }
    }




    #region Helpers

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