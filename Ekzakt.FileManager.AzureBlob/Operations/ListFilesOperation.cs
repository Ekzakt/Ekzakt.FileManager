﻿using Azure.Storage.Blobs;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Operations;

public class ListFilesOperation : AbstractFileOperation<ListFilesOperation>, IFileOperation<ListFilesRequest, IEnumerable<FileProperties>?>
{
    private readonly ILogger<ListFilesOperation> _logger;
    private readonly ListFilesRequestValidator _listFilesValidator;


    public ListFilesOperation(
        ILogger<ListFilesOperation> logger, 
        BlobServiceClient blobServiceClient,
        ListFilesRequestValidator listFilesValidator) : base(logger, blobServiceClient)
    {
        _logger = logger;
        _listFilesValidator = listFilesValidator;
    }


    public async Task<FileResponse<IEnumerable<FileProperties>?>> ExecuteAsync(ListFilesRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _listFilesValidator, out FileResponse<IEnumerable<FileProperties>?> validationResponse))
        {
            return validationResponse;
        }


        if (!EnsureBlobContainerClient<IEnumerable<FileProperties>?>(request!.BaseLocation, out FileResponse<IEnumerable<FileProperties>?> blobContainerResponse))
        {
            return blobContainerResponse;
        }


        try
        {
            _logger.LogRequestStarted(request);

            List<FileProperties> filesList = [];

            await foreach (var blobItem in BlobContainerClient!.GetBlobsAsync(
                prefix: request.GetPathsString(),
                cancellationToken: cancellationToken))
            {
                if (blobItem.Properties != null)
                {
                    var createdOn = blobItem.Properties.CreatedOn;

                    filesList.Add(new FileProperties
                    {
                        Name = blobItem.Name,
                        Size = blobItem.Properties.ContentLength ?? 0,
                        CreatedOn = createdOn?.DateTime
                    });
                }
            }

            _logger.LogInformation("Blobclients retrieved successfully.");

            return new FileResponse<IEnumerable<FileProperties>?>
            {
                Status = filesList.Count > 0 ? HttpStatusCode.OK : HttpStatusCode.NoContent,
                Message = filesList.Count > 0 ? "File list retreived successfully." : "No files where found.",
                Data = filesList.OrderByDescending(x => x.CreatedOn).AsEnumerable()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while retreiving filelist from blob container {BlobContainerName}. Exception {Exception}", request!.BaseLocation, ex);

            return new FileResponse<IEnumerable<FileProperties>?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "File list could not be retreived."
            };
        }
    }
}
