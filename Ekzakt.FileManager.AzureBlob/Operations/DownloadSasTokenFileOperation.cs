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

public class DownloadSasTokenFileOperation : AbstractFileOperation<DownloadSasTokenFileOperation>, IFileOperation<DownloadSasTokenRequest, DownloadSasTokenResponse?>
{
    private readonly ILogger<DownloadSasTokenFileOperation> _logger;
    private readonly DownloadSasTokenRequestValidator _validator;

    public DownloadSasTokenFileOperation(
        ILogger<DownloadSasTokenFileOperation> logger, 
        DownloadSasTokenRequestValidator validator,
        BlobServiceClient blobServiceClient) : base(logger, blobServiceClient)
    {
        _logger = logger;
        _validator = validator;
    }

    public async Task<FileResponse<DownloadSasTokenResponse?>> ExecuteAsync(DownloadSasTokenRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _validator, out FileResponse<DownloadSasTokenResponse?> validationResponse))
        {
            return validationResponse!;
        }


        if (!EnsureBlobContainerClient<DownloadSasTokenResponse?>(request!.BaseLocation, out FileResponse<DownloadSasTokenResponse?> blobContainerResponse))
        {
            return blobContainerResponse;
        }


        FileResponse<DownloadSasTokenResponse?> response = await Task.Run(() =>
        {
            try
            {
                _logger.LogRequestStarted(request);

                var userDelegationKey = BlobServiceClient
                    .GetUserDelegationKey(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(4), cancellationToken);

                var blobClient = BlobContainerClient!
                    .GetBlobClient(request!.FileName);

                request.Paths.Add(request!.FileName);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = request!.BaseLocation,
                    BlobName = request.GetPathsString(),
                    Resource = "b", // "b" for blob, "c" for container
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1),
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
                {
                    Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, BlobServiceClient.AccountName)
                };

                var downloadResponse = new DownloadSasTokenResponse()
                {
                    DownloadUri = blobUriBuilder.ToUri(),
                    ExpiresOn = sasBuilder.ExpiresOn

                };

                var sasToken = blobUriBuilder.ToUri().ToString();

                _logger.LogInformation("The download token is generated successfyly.");

                return new FileResponse<DownloadSasTokenResponse?>
                {
                    Status = HttpStatusCode.Created,
                    Message = "The download uri was created successfully.",
                    Data = downloadResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occured while generating the doownload token. Exception {Exception}", ex);

                return new FileResponse<DownloadSasTokenResponse?>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "File download uri could not be generated."
                };
            }
        });


        return response;
        
    }
}
