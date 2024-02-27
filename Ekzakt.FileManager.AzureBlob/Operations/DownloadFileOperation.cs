using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Operations;

public class DownloadFileOperation : AbstractFileOperation<DownloadFileOperation>, IFileOperation<DownloadFileRequest, DownloadFileResponse?>
{
    private readonly ILogger<DownloadFileOperation> _logger;
    private readonly DownloadFileRequestValidator _validator;

    public DownloadFileOperation(
        ILogger<DownloadFileOperation> logger, 
        DownloadFileRequestValidator validator,
        BlobServiceClient blobServiceClient) : base(logger, blobServiceClient)
    {
        _logger = logger;
        _validator = validator;
    }

    public async Task<FileResponse<DownloadFileResponse?>> ExecuteAsync(DownloadFileRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _validator, out FileResponse<DownloadFileResponse?> validationResponse))
        {
            return validationResponse!;
        }


        if (!EnsureBlobContainerClient<DownloadFileResponse?>(request!.BlobContainerName, out FileResponse<DownloadFileResponse?> blobContainerResponse))
        {
            return blobContainerResponse;
        }


        FileResponse<DownloadFileResponse?> response = await Task.Run(() =>
        {
            try
            {
                _logger.LogRequestStarted(request);

                var userDelegationKey = BlobServiceClient
                    .GetUserDelegationKey(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(4), cancellationToken);

                var blobClient = BlobContainerClient!
                    .GetBlobClient(request!.FileName);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = request!.BlobContainerName,
                    BlobName = request!.FileName,
                    Resource = "b", // "b" for blob, "c" for container
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1),
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
                {
                    Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, BlobServiceClient.AccountName)
                };

                var downloadResponse = new DownloadFileResponse()
                {
                    DownloadUri = blobUriBuilder.ToUri(),
                    ExpiresOn = sasBuilder.ExpiresOn

                };

                var sasToken = blobUriBuilder.ToUri().ToString();

                _logger.LogInformation("The download token is generated successfyly.");

                return new FileResponse<DownloadFileResponse?>
                {
                    Status = HttpStatusCode.Created,
                    Message = "The download uri was created successfully.",
                    Data = downloadResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occured while generating the doownload token. Exception {Exception}", ex);

                return new FileResponse<DownloadFileResponse?>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "File download uri could not be generated."
                };
            }
        });


        return response;
        
    }
}
