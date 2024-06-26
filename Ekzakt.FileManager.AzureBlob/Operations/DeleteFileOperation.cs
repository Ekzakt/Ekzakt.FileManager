﻿using Azure.Storage.Blobs;
using Ekzakt.FileManager.AzureBlob.Configuration;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Operations;

internal class DeleteFileOperation : AbstractAzureFileOperation<DeleteFileOperation>, IFileOperation<DeleteFileRequest, string?>
{
    private readonly ILogger<DeleteFileOperation> _logger;
    private EkzaktFileManagerAzureOptions _options;
    private DeleteFileRequestValidator _validator;

    public DeleteFileOperation(
        ILogger<DeleteFileOperation> logger,
        IOptions<EkzaktFileManagerAzureOptions> options,
        DeleteFileRequestValidator validator,
        BlobServiceClient blobServiceClient) : base(logger, blobServiceClient)
    {
        _logger = logger;
        _options = options.Value;
        _validator = validator;
    }

    public async Task<FileResponse<string?>> ExecuteAsync(DeleteFileRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _validator, out FileResponse<string?> validationResponse, _options))
        {
            return validationResponse;
        }

        if (!EnsureBlobContainerClient<string?>(request!.BaseLocation, out FileResponse<string?> blobContainerResponse))
        {
            return blobContainerResponse;
        }


        try
        {
            _logger.LogRequestStarted(request);

            request.Paths.Add(request!.FileName);

            var deleteResult = await BlobContainerClient!.DeleteBlobIfExistsAsync(request!.GetPathsString(), cancellationToken: cancellationToken);

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
