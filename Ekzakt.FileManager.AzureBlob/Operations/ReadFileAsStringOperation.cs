using Azure.Storage.Blobs;
using Ekzakt.FileManager.AzureBlob.Exceptions;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Operations;

public class ReadFileAsStringOperation : AbstractFileOperation<ReadFileAsStringOperation>, IFileOperation<ReadFileAsStringRequest, string?>
{
    private readonly ILogger<ReadFileAsStringOperation> _logger;
    private readonly ReadFileRequestValidator _validator;

    public ReadFileAsStringOperation(
        ILogger<ReadFileAsStringOperation> logger,
        ReadFileRequestValidator validator,
        BlobServiceClient blobServiceClient) : base(logger, blobServiceClient)
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

        if (!EnsureBlobContainerClient<string?>(request!.BlobContainerName, out FileResponse<string?> blobContainerResponse))
        {
            return blobContainerResponse;
        }

        try
        {
            _logger.LogRequestStarted(request);

            var prefixes = string.Join('/', request.Prefixes);

            var blobClient = BlobContainerClient?.GetBlobClient($"{prefixes}/{request!.FileName}");

            if (blobClient is null)
            {
                throw new ArgumentNullException(nameof(blobClient));
            }

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                throw new BlobClientNotFoundException(request!.FileName, request!.BlobContainerName);
            }

            var data = await ReadBlobClient(blobClient);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.OK,
                Data = data,
                Message = "File successfully read."
            };
        }
        catch (BlobClientNotFoundException ex)
        {
            _logger.LogError("The file {FileName} does not exists in container {BlobContainer}. Exception: {Exception}", request!.FileName, request!.BlobContainerName, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.NotFound,
                Message = "File not found."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while reading the file {FileName} in container {BlobContainer}. Exception {Exception}", request!.FileName, request!.BlobContainerName, ex);

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




    #region Helpers

    internal async Task<string?> ReadBlobClient(BlobClient blobClient)
    {
        using var stream = new MemoryStream();

        await blobClient.DownloadToAsync(stream);

        stream.Position = 0;

        using var streamReader = new StreamReader(stream);

        var result = await streamReader.ReadToEndAsync();

        return result;
    }

    #endregion Helpers
}
