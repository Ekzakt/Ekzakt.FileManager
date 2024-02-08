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
using Azure.Storage.Sas;

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
        private DeleteFileRequest? _deleteFileRequest;
        private DownloadFileRequest? _downloadFileRequest;


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

                _logger.LogInformation("The file {FileName} created successfully. CorrelationId: {CorrelationId}", _saveFileRequest!.FileName, _saveFileRequest!.CorrelationId);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.Created,
                    Message = "File created successfully.",
                    CorrelationId = _saveFileRequest.CorrelationId
                };
            }
            catch (BlobClientExistsException ex)
            {
                _logger.LogError("The file {FileName} already exists. It has not been overwritten. CorrelationId: {CorrelationId}. Exception: {Exception}", _saveFileRequest!.FileName, _saveFileRequest!.CorrelationId, ex);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.BadRequest,
                    Message = "The file already exists. It has not been overwritten.",
                    CorrelationId = _saveFileRequest!.CorrelationId
                };
            }
            catch (Exception ex) 
            {
                _logger.LogError("An error occured while saving the file {FileName}. CorrelationId: {CorrelationId}. Exception {Exception}", _saveFileRequest!.FileName, _saveFileRequest!.CorrelationId, ex);

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
                        var createdOn = blobItem.Properties.CreatedOn;

                        filesList.Add(new FileInformation
                        {
                            Name = blobItem.Name,
                            Size = blobItem.Properties.ContentLength ?? 0,
                            CreatedOn = createdOn?.DateTime
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


        public async Task<FileResponse<string?>> DeleteFileAsync<T>(T deleteFileRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest
        {
            _deleteFileRequest = deleteFileRequest as DeleteFileRequest;


            if (!ValidateRequest(_deleteFileRequest!, _deleteFileValidator, out FileResponse<string?> validationResponse))
            {
                return validationResponse;
            }


            if (!EnsureBlobContainer<string?>(_deleteFileRequest!.ContainerName, _deleteFileRequest!.CorrelationId, out FileResponse<string?> blobContainerResponse))
            {
                return blobContainerResponse;
            }


            try
            {
                _logger.LogRequestStarted(_deleteFileRequest);

                var deleteResult = await _blobContainerClient!.DeleteBlobIfExistsAsync(_deleteFileRequest!.FileName, cancellationToken: cancellationToken);

                if (deleteResult == true)
                {
                    _logger.LogInformation("The file {FileName} was deleted successfully. CorrelationId: {CorrelationId}", _deleteFileRequest!.FileName, _deleteFileRequest!.CorrelationId);

                    return new FileResponse<string?>
                    {
                        Status = HttpStatusCode.OK,
                        Message = "The file was deleted succuessfully.",
                        CorrelationId = _deleteFileRequest.CorrelationId
                    };
                }

                _logger.LogInformation("The specified file does not exist. CorrelationId: {CorrelationId}", _deleteFileRequest!.CorrelationId);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.NotFound,
                    Message = "The specified file does not exist.",
                    CorrelationId = _deleteFileRequest.CorrelationId
                };

            }
            catch (Exception ex)
            {
                _logger.LogError("An error occured while deleteing the file {FileName}. CorrelationId: {CorrelationId}. Exception {Exception}", _deleteFileRequest!.FileName, _listFilesRequest!.CorrelationId, ex);

                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "The file could not be deleted.",
                    CorrelationId = _listFilesRequest!.CorrelationId
                };
            }
        }


        public FileResponse<DownloadFileResponse?> DownloadFile<T>(T downloadFileRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest
        {
            _downloadFileRequest = downloadFileRequest as DownloadFileRequest;

            if (!ValidateRequest(_downloadFileRequest!, _downloadFileValidator, out FileResponse<DownloadFileResponse?> validationResponse))
            {
                return validationResponse!;
            }


            if (!EnsureBlobContainer<DownloadFileResponse?>(_downloadFileRequest!.ContainerName, _downloadFileRequest!.CorrelationId, out FileResponse<DownloadFileResponse?> blobContainerResponse))
            {
                return blobContainerResponse;
            }

            try
            {
                _logger.LogRequestStarted(_downloadFileRequest);

                var userDelegationKey = _blobServiceClient
                    .GetUserDelegationKey(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(4), cancellationToken);

                var blobClient = _blobContainerClient!
                    .GetBlobClient(_downloadFileRequest!.FileName);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _downloadFileRequest!.ContainerName,
                    BlobName = _downloadFileRequest!.FileName,
                    Resource = "b", // "b" for blob, "c" for container
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(1),
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var blobUriBuilder = new BlobUriBuilder(blobClient.Uri)
                {
                    Sas = sasBuilder.ToSasQueryParameters(userDelegationKey, _blobServiceClient.AccountName)
                };


                var downloadResponse = new DownloadFileResponse()
                { 
                    DownloadUri = blobUriBuilder.ToUri(),
                    ExpiresOn = sasBuilder.ExpiresOn

                };
                

                var sasToken = blobUriBuilder.ToUri().ToString();

                _logger.LogInformation("The download token is generated successfyly. CorrelationId {CorrelationId}.", _downloadFileRequest!.CorrelationId);

                return new FileResponse<DownloadFileResponse?>
                {
                    Status = HttpStatusCode.Created,
                    Message = "The download uri was created successfully.",
                    Data = downloadResponse,
                    CorrelationId = _downloadFileRequest.CorrelationId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occured while generating the doownload token. CorrelationId: {CorrelationId}. Exception {Exception}", _downloadFileRequest!.CorrelationId, ex);

                return new FileResponse<DownloadFileResponse?>
                {
                    Status = HttpStatusCode.InternalServerError,
                    Message = "File download uri could not be generated.",
                    CorrelationId = _downloadFileRequest!.CorrelationId
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