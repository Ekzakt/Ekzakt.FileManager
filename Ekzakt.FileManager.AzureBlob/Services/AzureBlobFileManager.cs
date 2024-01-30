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
using Ekzakt.FileManager.Core.Models;
using System.Linq;

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

        private BlobContainerClient? _blobContainerClient;

        private SaveFileRequest? _saveFileRequest;
        private ListFilesRequest? _listFilesRequest;


        public AzureBlobFileManager(
            ILogger<AzureBlobFileManager> logger,
            BlobServiceClient blobServiceClient,
            SaveFileRequestValidator saveFileValidator,
            DownloadFileRequestValidator downloadFileValidator,
            ListFilesRequestValidator listFilesValidator,
            DeleteFileRequestValidator deleteFileValidator)
            : base(logger)
        {
            _logger = logger;
            _blobServiceClient = blobServiceClient;
            _saveFileValidator = saveFileValidator;
            _downloadFileValidator = downloadFileValidator;
            _listFilesValidator = listFilesValidator;
            _deleteFileValidator = deleteFileValidator;
        }


        public async Task<FileResponse<string?>>SaveFileAsync<T>(T saveFileRequest, CancellationToken cancellationToken = default)
            where T : AbstractFileRequest
        {

            _saveFileRequest = saveFileRequest as SaveFileRequest;


            if (!ValidateRequest(_saveFileRequest!, _saveFileValidator, out FileResponse<string?> validationResponse))
            {
                return validationResponse!;
            }


            if (!EnsureBlobContainer<string?>(_saveFileRequest!.ContainerName, _saveFileRequest!.CorrelationId, out FileResponse<string?> blobContainerResponse))
            {
                return blobContainerResponse;
            }


            try
            {
                _logger.LogRequestStarted(_saveFileRequest);

                _saveFileRequest!.FileStream!.Position = 0;

                var blobClient = _blobContainerClient!.GetBlobClient(_saveFileRequest!.FileName);

                if (await blobClient.ExistsAsync(cancellationToken))
                {
                    throw new BlobClientExistsException(_saveFileRequest!.FileName, _saveFileRequest!.ContainerName);
                }

                await blobClient.UploadAsync(_saveFileRequest!.FileStream, GetBlobUploadOptions(), cancellationToken);

                _logger.LogInformation("File {FileName} created successfully. CorrelationId: {CorrelationId}", _saveFileRequest!.FileName, _saveFileRequest!.CorrelationId);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.Created,
                    Message = "File created successfully.",
                    CorrelationId = _saveFileRequest.CorrelationId
                };
            }
            catch (BlobClientExistsException ex)
            {
                _logger.LogError("File {FileName} already exists. CorrelationId: {CorrelationId}. Exception: {Exception}", _saveFileRequest!.FileName, _saveFileRequest!.CorrelationId, ex);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "File already exists. It has not been overwritten.",
                    CorrelationId = _saveFileRequest!.CorrelationId
                };
            }
            catch (Exception ex) 
            {
                _logger.LogError("An error occured while saving file {FileName}. CorrelationId: {CorrelationId}. Exception {Exception}", _saveFileRequest!.FileName, _saveFileRequest!.CorrelationId, ex);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "File could not be saved.",
                    CorrelationId = _saveFileRequest!.CorrelationId
                };
            }
            finally
            {
                _logger.LogInformation("Closing request-filestream. CorrelationId: {CorrellationId}.", _saveFileRequest!.CorrelationId);

                _saveFileRequest!.FileStream?.Close();
            }
        }


        public async Task<FileResponse<IEnumerable<FileInformation>?>> ListFilesAsync<T>(T listFilesRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest
        {
            _listFilesRequest = listFilesRequest as ListFilesRequest;


            if (!ValidateRequest(_listFilesRequest!, _listFilesValidator, out FileResponse<IEnumerable<FileInformation>?> validationResponse))
            {
                return validationResponse;
            }


            if (!EnsureBlobContainer<IEnumerable<FileInformation>?>(_listFilesRequest!.ContainerName, _listFilesRequest!.CorrelationId, out FileResponse<IEnumerable<FileInformation>?> blobContainerResponse))
            {
                return blobContainerResponse;
            }


            try
            {
                _logger.LogRequestStarted(_listFilesRequest);

                List<FileInformation> filesList = [];

                await foreach (var blobItem in _blobContainerClient!.GetBlobsAsync(cancellationToken: cancellationToken))
                {
                    if (blobItem.Properties != null)
                    {
                        filesList.Add(new FileInformation
                        {
                            Name = blobItem.Name,
                            Size = blobItem.Properties.ContentLength ?? 0
                        });
                    }
                }

                _logger.LogInformation("Blobclients retieved successfully. CorrelationId: {CorrelationId}", _listFilesRequest!.CorrelationId);

                return new FileResponse<IEnumerable<FileInformation>?>
                {
                    Status = filesList.Count > 0 ? HttpStatusCode.OK : HttpStatusCode.NoContent,
                    Message = filesList.Count > 0 ? "File list retreived successfully." : "No filelist could be created.",
                    Data = filesList.OrderBy(x => x.Name).AsEnumerable(),
                    CorrelationId = _listFilesRequest.CorrelationId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occured while retreiving filelist from blob container {BlobContainerName}. CorrelationId: {CorrelationId}. Exception {Exception}", _listFilesRequest!.ContainerName, _listFilesRequest!.CorrelationId, ex);

                return new FileResponse<IEnumerable<FileInformation>?>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "File list could not be retreived.",
                    CorrelationId = _listFilesRequest!.CorrelationId
                };
            }
        }




        #region Helpers

        private BlobUploadOptions GetBlobUploadOptions()
        {
            var blobOptions = new BlobUploadOptions
            {
                ProgressHandler = new FileProgressHandler(_saveFileRequest!),
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
            if (_blobContainerClient is not null && _blobContainerClient.Name.Equals(containerName))
            {
                response = new();
                return true;
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
                    Message = "Blob container not found.",
                    CorrelationId = _saveFileRequest!.CorrelationId
                };

                return false;
            }
        }


        #endregion Helpers
    }
}