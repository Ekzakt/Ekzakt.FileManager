using Azure;

namespace Ekzakt.FileManager.AzureBlob.Exceptions;

public class BlobClientExistsException : Exception
{
    public string BlobClientName { get; }
    public string BlobContainerName { get; }

    public BlobClientExistsException(string blobClientName, string blobContainerName)
        : base($"Blob with clientname '{blobClientName}' already exists in container '{blobContainerName}'.")
    {
        BlobClientName = blobClientName ?? throw new ArgumentNullException(nameof(blobClientName));
        BlobContainerName = blobContainerName ?? throw new ArgumentNullException(nameof(blobContainerName));
    }
}