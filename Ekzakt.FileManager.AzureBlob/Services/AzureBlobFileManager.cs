using Azure.Storage.Blobs;
using Azure;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using System.Net;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Azure.Storage.Blobs.Models;
using Azure.Storage;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.AzureBlob.Exceptions;

namespace Ekzakt.FileManager.AzureBlob.Services
{
    public class AzureBlobFileManager : AbstractFileManager, IFileManager
    {
        private readonly ILogger<AzureBlobFileManager> _logger;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly SaveFileRequestValidator _saveFileValidator;
        private readonly DownloadFileRequestValidator _downloadFileValidator;
        private readonly ListFilesRequestValidator _listFilesValidator;
        private readonly DeleteFileRequestValidator _deleteFileValidator;

        private SaveFileRequest? _request;
        private BlobContainerClient? _blobContainerClient;


        public AzureBlobFileManager(
            ILogger<AzureBlobFileManager> logger,
            BlobServiceClient blobServiceClient,
            SaveFileRequestValidator saveFileValidator,
            DownloadFileRequestValidator downloadFileValidator,
            ListFilesRequestValidator listFilesValidator,
            DeleteFileRequestValidator deleteFileValidator)
            : base(logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(saveFileValidator));
            _saveFileValidator = saveFileValidator ?? throw new ArgumentNullException(nameof(saveFileValidator));
            _downloadFileValidator = downloadFileValidator ?? throw new ArgumentNullException(nameof(downloadFileValidator));
            _listFilesValidator = listFilesValidator ?? throw new ArgumentNullException(nameof(listFilesValidator));
            _deleteFileValidator = deleteFileValidator ?? throw new ArgumentNullException(nameof(deleteFileValidator));
        }


        public async Task<FileResponse<string?>>SaveFileAsync<T>(T saveFileRequest, CancellationToken cancellationToken = default)
            where T : AbstractFileRequest
        {

            _request = saveFileRequest as SaveFileRequest;

            if (!ValidateRequest(_request!, _saveFileValidator, out FileResponse<string?> validationResponse))
            {
                return validationResponse!;
            }

            try
            {
                _logger.LogRequestStarted(_request);

                _request!.FileStream!.Position = 0;

                if (!EnsureBlobContainer<string?>(_request.ContainerName, _request!.CorrelationId, out FileResponse<string?> response))
                {
                    return response;
                }

                var blobClient = _blobContainerClient!.GetBlobClient(_request!.FileName);

                if (await blobClient.ExistsAsync(cancellationToken))
                {
                    throw new BlobClientExistsException(_request!.FileName, _request!.ContainerName);
                }

                await blobClient.UploadAsync(_request!.FileStream, GetBlobUploadOptions(), cancellationToken);

                _logger.LogInformation("File {FileName} successfully created. CorrelationId: {CorrelationId}", _request!.FileName, _request!.CorrelationId);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.Created,
                    Message = "File created successfully.",
                    CorrelationId = _request.CorrelationId
                };
            }
            catch (BlobClientExistsException ex)
            {
                _logger.LogError("File {FileName} already exists. CorrelationId: {CorrelationId}. Exception: {Exception}", _request!.FileName, _request!.CorrelationId, ex);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "File already exists. It has not been overwritten.",
                    CorrelationId = _request!.CorrelationId
                };
            }
            catch (Exception ex) 
            {
                _logger.LogError("An error occured while saving file {FileName}. CorrelationId: {CorrelationId}. Exception {Exception}", _request!.FileName, _request!.CorrelationId, ex);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "File could not be saved.",
                    CorrelationId = _request!.CorrelationId
                };
            }
            finally
            {
                _logger.LogInformation("Closing request-filestream. CorrelationId: {CorrellationId}.", _request!.CorrelationId);

                _request!.FileStream?.Close();
            }
        }


        #region Helpers


        private BlobUploadOptions GetBlobUploadOptions()
        {
            var blobOptions = new BlobUploadOptions
            {
                ProgressHandler = new FileProgressHandler(_request!),
                TransferOptions = new StorageTransferOptions
                {
                    MaximumConcurrency = Environment.ProcessorCount * 2,
                    MaximumTransferSize = 50 * 1024 * 1024
                }
            };

            return blobOptions;
        }


        private bool EnsureBlobContainer<TResponse>(string containerName, Guid correlationId, out FileResponse<TResponse?> response)
            where TResponse : class?
        {
            if (_blobContainerClient is not null)
            {
                if (_blobContainerClient.Name.Equals(containerName))
                {
                    response = new();
                    return true;
                }
            }

            try
            {
                _blobContainerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                response = new();
                return true;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                _logger.LogError("Blobcontainer {BlobContainerName} does not exist. CorrelationId {CorrelationId}. Exception: {Exception}", containerName, correlationId, ex);

                response = new FileResponse<TResponse?>
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "Error saving file. Container not found.",
                    CorrelationId = _request!.CorrelationId
                };

                return false;
            }
        }


        #endregion Helpers
    }
}