using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Ekzakt.FileManager.AzureBlob.Configuration;
using Ekzakt.FileManager.AzureBlob.Exceptions;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Ekzakt.FileManager.AzureBlob.Services;

public class AzureBlobFileManager : AbstractFileManager, IFileManager
{
    private readonly ILogger<AzureBlobFileManager> _logger;
    private readonly FileManagerOptions _fileManangerOptions;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly SaveFileRequestValidator _saveFileValidator;
    private readonly SaveChunkedFileRequestValidator _saveChunkedFileValidator;
    private readonly DownloadFileRequestValidator _downloadFileValidator;
    private readonly ListFilesRequestValidator _listFilesValidator;
    private readonly DeleteFileRequestValidator _deleteFileValidator;

    private BlobContainerClient? _blobContainerClient;

    private SaveFileRequest? _saveFileRequest;
    private SaveChunkedFileRequest? _saveChunkedFileRequest;
    private ListFilesRequest? _listFilesRequest;
    private DeleteFileRequest? _deleteFileRequest;
    private DownloadFileRequest? _downloadFileRequest;

    static List<string> blockBlobIds = new();

    public AzureBlobFileManager(
        ILogger<AzureBlobFileManager> logger,
        IOptions<FileManagerOptions> fileManangerOptions,
        BlobServiceClient blobServiceClient,
        SaveFileRequestValidator saveFileValidator,
        SaveChunkedFileRequestValidator saveChunkedFileValidator,
        DownloadFileRequestValidator downloadFileValidator,
        ListFilesRequestValidator listFilesValidator,
        DeleteFileRequestValidator deleteFileValidator)
        : base(logger)
    {
        _logger = logger;
        _fileManangerOptions = fileManangerOptions.Value;
        _blobServiceClient = blobServiceClient;
        _saveFileValidator = saveFileValidator;
        _saveChunkedFileValidator = saveChunkedFileValidator;
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


        if (!EnsureBlobContainerClient<string?>(_saveFileRequest!.ContainerName, out FileResponse<string?> blobContainerResponse))
        {
            return blobContainerResponse;
        }


        try
        {
            _logger.LogRequestStarted(_saveFileRequest);

            _saveFileRequest!.FileStream!.Position = 0;

            var blobClient = _blobContainerClient!.GetBlobClient(_saveFileRequest!.OriginalFilename);

            if (await blobClient.ExistsAsync(cancellationToken))
            {
                throw new BlobClientExistsException(_saveFileRequest!.OriginalFilename, _saveFileRequest!.ContainerName);
            }

            await blobClient.UploadAsync(_saveFileRequest!.FileStream, GetBlobUploadOptions(), cancellationToken);

            _logger.LogInformation("The file {FileName} created successfully.", _saveFileRequest!.OriginalFilename);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.Created,
                Message = "File created successfully."
            };

        }
        catch (BlobClientExistsException ex)
        {
            _logger.LogError("The file {FileName} already exists. It has not been overwritten. Exception: {Exception}", _saveFileRequest!.OriginalFilename, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.BadRequest,
                Message = "The file already exists. It has not been overwritten."
            };
        }
        catch (Exception ex) 
        {
            _logger.LogError("An error occured while saving the file {FileName}. Exception {Exception}", _saveFileRequest!.OriginalFilename, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "File could not be saved."
            };
        }
        finally
        {
            _logger.LogInformation("Closing request-filestream.");

            _saveFileRequest!.FileStream?.Close();
        }
    }


    public async Task<FileResponse<string?>> SaveFileChunkedAsync<T>(T saveFileRequest, CancellationToken cancellationToken = default)
        where T : AbstractFileRequest
    {

        _saveChunkedFileRequest = saveFileRequest as SaveChunkedFileRequest;


        if (!ValidateRequest(_saveChunkedFileRequest!, _saveChunkedFileValidator, out FileResponse<string?> validationResponse))
        {
            return validationResponse!;
        }


        if (!EnsureBlobContainerClient<string?>(_saveChunkedFileRequest!.ContainerName, out FileResponse<string?> blobContainerResponse))
        {
            return blobContainerResponse;
        }


        try
        {
            if (_saveChunkedFileRequest!.ChunkIndex == 0)
            {
                _logger.LogRequestStarted(_saveChunkedFileRequest);
            }

            var blockBlobClient = _blobContainerClient!.GetBlockBlobClient(_saveChunkedFileRequest!.OriginalFilename);

            if (await blockBlobClient.ExistsAsync(cancellationToken))
            {
                throw new BlobClientExistsException(_saveChunkedFileRequest!.OriginalFilename, _saveChunkedFileRequest!.ContainerName);
            }

            if (_saveChunkedFileRequest!.ChunkIndex == 0)
            {
                blockBlobIds = new();
            }

            var blockId = BitConverter.GetBytes(_saveChunkedFileRequest!.ChunkIndex);
            var base64BlockId = Convert.ToBase64String(blockId);

            blockBlobIds.Add(base64BlockId);

            using var ms = new MemoryStream(_saveChunkedFileRequest!.ChunkData!);

            await blockBlobClient.StageBlockAsync(base64BlockId, ms, cancellationToken: cancellationToken);


            if (_saveChunkedFileRequest.Commit)
            {
                await blockBlobClient.CommitBlockListAsync(blockBlobIds);

                blockBlobIds = new();

                _logger.LogInformation("The file {FileName} created successfully.", _saveChunkedFileRequest!.OriginalFilename);

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
            _logger.LogError("The file {FileName} already exists. It has not been overwritten. Exception: {Exception}", _saveChunkedFileRequest!.OriginalFilename, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.BadRequest,
                Message = "The file already exists. It has not been overwritten."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while saving the file {FileName}. Exception {Exception}", _saveChunkedFileRequest!.OriginalFilename, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "File could not be saved."
            };
        }
    }


    public async Task<FileResponse<IEnumerable<FileInformation>?>> ListFilesAsync<T>(T listFilesRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest
    {
        _listFilesRequest = listFilesRequest as ListFilesRequest;


        if (!ValidateRequest(_listFilesRequest!, _listFilesValidator, out FileResponse<IEnumerable<FileInformation>?> validationResponse))
        {
            return validationResponse;
        }


        if (!EnsureBlobContainerClient<IEnumerable<FileInformation>?>(_listFilesRequest!.ContainerName, out FileResponse<IEnumerable<FileInformation>?> blobContainerResponse))
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

            _logger.LogInformation("Blobclients retrieved successfully.");

            return new FileResponse<IEnumerable<FileInformation>?>
            {
                Status = filesList.Count > 0 ? HttpStatusCode.OK : HttpStatusCode.NoContent,
                Message = filesList.Count > 0 ? "File list retreived successfully." : "No filelist could be created.",
                Data = filesList.OrderBy(x => x.Name).AsEnumerable()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while retreiving filelist from blob container {BlobContainerName}. Exception {Exception}", _listFilesRequest!.ContainerName, ex);

            return new FileResponse<IEnumerable<FileInformation>?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "File list could not be retreived."
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


        if (!EnsureBlobContainerClient<string?>(_deleteFileRequest!.ContainerName, out FileResponse<string?> blobContainerResponse))
        {
            return blobContainerResponse;
        }


        try
        {
            _logger.LogRequestStarted(_deleteFileRequest);

            var deleteResult = await _blobContainerClient!.DeleteBlobIfExistsAsync(_deleteFileRequest!.FileName, cancellationToken: cancellationToken);

            if (deleteResult == true)
            {
                _logger.LogInformation("The file {FileName} was deleted successfully.", _deleteFileRequest!.FileName);

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
            _logger.LogError("An error occured while deleteing the file {FileName}. Exception {Exception}", _deleteFileRequest!.FileName, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "The file could not be deleted."
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


        if (!EnsureBlobContainerClient<DownloadFileResponse?>(_downloadFileRequest!.ContainerName, out FileResponse<DownloadFileResponse?> blobContainerResponse))
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
                InitialTransferSize = _fileManangerOptions.Upload.InitialTransferSize,
                MaximumTransferSize = _fileManangerOptions.Upload.MaximumTransferSize
            }
        };

        return blobOptions;
    }


    private bool EnsureBlobContainerClient<TResponse>(string containerName, out FileResponse<TResponse?> response)
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
            _logger.LogError("Blobcontainer {BlobContainerName} does not exist. Exception: {Exception}", containerName, ex);

            response = new FileResponse<TResponse?>
            {
                Status = HttpStatusCode.NotFound,
                Message = "Blob container not found."
            };

            return false;
        }
    }


    #endregion Helpers
}