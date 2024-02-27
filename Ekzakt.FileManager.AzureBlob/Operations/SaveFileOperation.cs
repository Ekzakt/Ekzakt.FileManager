using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Ekzakt.FileManager.AzureBlob.Exceptions;
using Ekzakt.FileManager.AzureBlob.Services;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Options;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Operations;

public class SaveFileOperation : AbstractFileOperation<SaveFileOperation>, IFileOperation<SaveFileRequest, string?>
{
    private readonly ILogger<SaveFileOperation> _logger;
    private readonly SaveFileRequestValidator _validator;
    private readonly FileManagerOptions _options;


    public SaveFileOperation(
        ILogger<SaveFileOperation> logger,
        IOptions<FileManagerOptions> fileManagerOptions,
        SaveFileRequestValidator validator,
        BlobServiceClient blobServiceClient) : base(logger, blobServiceClient)
    {
        _logger = logger;
        _validator = validator;
        _options = fileManagerOptions.Value;
    }


    public async Task<FileResponse<string?>> ExecuteAsync(SaveFileRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _validator, out FileResponse<string?> validationResponse))
        {
            return validationResponse!;
        }

        if (!EnsureBlobContainerClient<string?>(request!.BlobContainerName, out FileResponse<string?> blobContainerResponse))
        {
            return blobContainerResponse;
        }

        try
        {
            _logger.LogRequestStarted(request);

            request!.FileStream!.Position = 0;

            var blobClient = BlobContainerClient?.GetBlobClient(request!.FileName);

            if (blobClient is null)
            {
                throw new ArgumentNullException(nameof(blobClient));
            }

            if (await blobClient.ExistsAsync(cancellationToken))
            {
                throw new BlobClientExistsException(request!.FileName, request!.BlobContainerName);
            }

            await blobClient.UploadAsync(request!.FileStream, GetBlobUploadOptions(request), cancellationToken);

            _logger.LogInformation("The file {FileName} created successfully in container {BlobContainer}.", request!.FileName, request!.BlobContainerName);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.Created,
                Message = "File created successfully."
            };
        }
        catch (BlobClientExistsException ex)
        {
            _logger.LogError("The file {FileName} already exists in container {BlobContainer}. It has not been overwritten. Exception: {Exception}", request!.FileName, request!.BlobContainerName, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.BadRequest,
                Message = "The file already exists. It has not been overwritten."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while saving the file {FileName} in container {BlobContainer}. Exception {Exception}", request!.FileName, request!.BlobContainerName, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "File could not be saved."
            };
        }
        finally
        {
            _logger.LogInformation("Closing request-filestream.");

            request!.FileStream?.Close();
            request!.FileStream?.Dispose();
        }
    }




    #region Helpers

    private BlobUploadOptions GetBlobUploadOptions(SaveFileRequest request)
    {
        var blobOptions = new BlobUploadOptions
        {
            ProgressHandler = new FileProgressHandler(request),
            TransferOptions = new StorageTransferOptions
            {
                MaximumConcurrency = Environment.ProcessorCount * 2,
                InitialTransferSize = _options.Upload.InitialTransferSize,
                MaximumTransferSize = _options.Upload.MaximumTransferSize
            }
        };

        return blobOptions;
    }


    #endregion Helpers
}
