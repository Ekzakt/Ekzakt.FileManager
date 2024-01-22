using Azure.Identity;
using Azure.Storage.Blobs;
using Ekzakt.FileManager.AzureBlob.Configuration;
using Ekzakt.FileManager.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Azure.Storage.Blobs.Models;
using Azure.Storage;
using Ekzakt.FileManager.Core.Models;

namespace Ekzakt.FileManager.AzureBlob.Services;

public class AzureBlobFileManager : IFileManager
{
    private readonly ILogger<AzureBlobFileManager> _logger;
    private readonly AzureFileManagerOptions _options;

    private BlobContainerClient _blobContainerClient;
    private Progress<long> _progressHandler = new();

    public event EventHandler ProgressEventHandler;

    protected virtual void OnProgressChanged(EventArgs e)
    {
        ProgressEventHandler?.Invoke(this, e);
    }

    public AzureBlobFileManager(
        ILogger<AzureBlobFileManager> logger,
        IOptions<AzureFileManagerOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }



    public async Task<SaveFileResponse> SaveAsync(string containerName, Stream inputStream, string fileName, string? contentType = null)
    {
        SetBlobContainerClient(containerName);

        BlobClient blobClient = _blobContainerClient.GetBlobClient(fileName);

        _progressHandler.ProgressChanged += UploadProgressChanged;

        inputStream.Position = 0;

        var blobOptions = new BlobUploadOptions()
        {
            //HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            ProgressHandler = _progressHandler,
            TransferOptions = new StorageTransferOptions()
            {
                MaximumConcurrency = Environment.ProcessorCount * 2,
                MaximumTransferSize = 50 * 1024 * 1024
            }
        };


        try
        {
            var blobResult = await blobClient.UploadAsync(content: inputStream, options: blobOptions);
            var output = new SaveFileResponse { RawResult = blobResult.ToString() };

            return output; 
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while saving file. {0}", ex);
            
            throw;
        }
        finally
        {
            _progressHandler.ProgressChanged -= UploadProgressChanged;
        }
    }

    #region Helpers

    private void UploadProgressChanged(object sender, long bytesUploaded)
    {
        OnProgressChanged(new ProgressEventArgs
        {
            BytesSent = bytesUploaded
        });
    }


    private void SetBlobContainerClient(string containerName)
    {
        if (_blobContainerClient is not null)
        {
            return;
        }

        var blobServiceClient = new BlobServiceClient(
            new Uri($"https://{_options.Azure.StorageAccount.Name}.blob.core.windows.net"),
            new DefaultAzureCredential());

        _blobContainerClient = blobServiceClient
            .GetBlobContainerClient(containerName);
    }

    #endregion Helpers
}
