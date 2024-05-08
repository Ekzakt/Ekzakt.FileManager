using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Ekzakt.FileManager.AzureBlob.Exceptions;
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

public class SaveFileChunkedOperation : AbstractFileOperation<SaveFileChunkedOperation>, IFileOperation<SaveFileChunkedRequest, string?>
{
    private readonly ILogger<SaveFileChunkedOperation> _logger;
    private readonly SaveChunkedFileRequestValidator _validator;

    static List<string> blockBlobIds = new();

    public SaveFileChunkedOperation(
        ILogger<SaveFileChunkedOperation> logger,
        IOptions<FileManagerOptions> options,
        SaveChunkedFileRequestValidator validator,
        BlobServiceClient blobServiceClient) : base(logger, options, blobServiceClient)
    {
        _logger = logger;
        _validator = validator;
    }


    public async Task<FileResponse<string?>> ExecuteAsync(SaveFileChunkedRequest request, CancellationToken cancellationToken = default)
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
            if (request!.ChunkIndex == 0)
            {
                _logger.LogRequestStarted(request);
            }

            request.Paths.Add(request!.FileName);

            var blockBlobClient = BlobContainerClient?.GetBlockBlobClient(request!.GetPathsString());

            if (blockBlobClient is null)
            {
                throw new ArgumentNullException(nameof(blockBlobClient));
            }

            if (await blockBlobClient.ExistsAsync(cancellationToken))
            {
                throw new BlobClientExistsException(request!.BaseLocation, request!.FileName);
            }

            if (request!.ChunkIndex == 0)
            {
                blockBlobIds = new();
            }

            var blockId = BitConverter.GetBytes(request!.ChunkIndex);
            var base64BlockId = Convert.ToBase64String(blockId);

            blockBlobIds.Add(base64BlockId);

            byte[] bytes = Convert.FromBase64String(request!.ChunkData);
            using var ms = new MemoryStream(bytes);

            await blockBlobClient.StageBlockAsync(base64BlockId, ms, cancellationToken: cancellationToken);

            if (request.Commit)
            {
                await blockBlobClient.CommitBlockListAsync(blockBlobIds);

                blockBlobIds = new();

                _logger.LogInformation("The file {FileName} created successfully.", request!.FileName);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.Continue,
                    Message = "File created successfully."
                };
            }
            else
            {
                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.Continue,
                    Message = "Continue uploading."
                };
            }
        }
        catch (BlobClientExistsException ex)
        {
            _logger.LogError("The file {FileName} already exists. It has not been overwritten. Exception: {Exception}", request!.FileName, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.BadRequest,
                Message = "The file already exists. It has not been overwritten."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while saving the file {FileName}. Exception {Exception}", request!.FileName, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "File could not be saved."
            };
        }
    }
}
