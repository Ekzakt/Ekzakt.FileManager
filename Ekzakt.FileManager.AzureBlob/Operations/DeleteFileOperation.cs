using Azure.Storage.Blobs;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Operations;

public class DeleteFileOperation : AbstractFileOperation<DeleteFileOperation>, IFileOperation<DeleteFileRequest, string?>
{
    private readonly ILogger<DeleteFileOperation> _logger;
    private DeleteFileRequestValidator _validator;


    public DeleteFileOperation(
        ILogger<DeleteFileOperation> logger,
        DeleteFileRequestValidator validator,
        BlobServiceClient blobServiceClient) : base(logger, blobServiceClient)
    {
        _logger = logger;
        _validator = validator;
    }

    public async Task<FileResponse<string?>> ExecuteAsync(DeleteFileRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _validator, out FileResponse<string?> validationResponse))
        {
            return validationResponse;
        }

        if (!EnsureBlobContainerClient<string?>(request!.BlobContainerName, out FileResponse<string?> blobContainerResponse))
        {
            return blobContainerResponse;
        }


        try
        {
            _logger.LogRequestStarted(request);

            var deleteResult = await BlobContainerClient!.DeleteBlobIfExistsAsync(request!.FileName, cancellationToken: cancellationToken);

            if (deleteResult == true)
            {
                _logger.LogInformation("The file {FileName} was deleted successfully.", request!.FileName);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.OK,
                    Message = "The file was deleted succuessfully."
                };
            }

            _logger.LogInformation("The specified file does not exist.");

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.NotFound,
                Message = "The specified file does not exist."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while deleteing the file {FileName}. Exception {Exception}", request!.FileName, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "The file could not be deleted."
            };
        }
    }
}
