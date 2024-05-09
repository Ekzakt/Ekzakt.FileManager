using Azure.Storage.Blobs;
using Ekzakt.FileManager.AzureBlob.Exceptions;
using Ekzakt.FileManager.AzureBlob.Extensions;
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

internal class ReadFileAsStringOperation : AbstractFileOperation<ReadFileAsStringOperation>, IFileOperation<ReadFileAsStringRequest, string?>
{
    private readonly ILogger<ReadFileAsStringOperation> _logger;
    private readonly ReadFileAsStringRequestValidator _validator;

    public ReadFileAsStringOperation(
        ILogger<ReadFileAsStringOperation> logger,
        IOptions<FileManagerOptions> options,
        ReadFileAsStringRequestValidator validator,
        BlobServiceClient blobServiceClient) : base(logger, options, blobServiceClient)
    {
        _logger = logger;
        _validator = validator;
    }


    public async Task<FileResponse<string?>> ExecuteAsync(ReadFileAsStringRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _validator, out FileResponse<string?> validationResponse))
        {
            return validationResponse!;
        }

        if (!EnsureBlobContainerClient<string?>(request!.BaseLocation, out FileResponse<string?> blobContainerResponse))
        {
            return blobContainerResponse;
        }

        try
        {
            _logger.LogRequestStarted(request);

            request.Paths.Add(request!.FileName);

            var blobClient = BlobContainerClient?.GetBlobClient(request.GetPathsString());

            if (blobClient is null)
            {
                throw new ArgumentNullException(nameof(blobClient));
            }

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                throw new BlobClientNotFoundException(request!.BaseLocation, request!.FileName, request.GetPathsString());
            }

            var data = await blobClient.ReadAsStringAsync();

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.OK,
                Data = data,
                Message = "File successfully read."
            };
        }
        catch (BlobClientNotFoundException ex)
        {
            _logger.LogError("The file {FileName} does not exists in container {BlobContainer}. Exception: {Exception}", request!.FileName, request!.BaseLocation, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.NotFound,
                Message = "File not found."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while reading the file {FileName} in container {BlobContainer}. Exception {Exception}", request!.FileName, request!.BaseLocation, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "File cound not be read."
            };
        }
        finally
        {
            _logger.LogRequestFinished(request);
        }
    }
}
