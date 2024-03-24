namespace Ekzakt.FileManager.AzureBlob.Exceptions;

public class BlobClientNotFoundException : Exception
{
    public string BlobClientName { get; }
    public string BlobContainerName { get; }

    public BlobClientNotFoundException(string blobContainerName, string blobClientName)
        : base($"Blob with clientname '{blobClientName}' does not exists in container '{blobContainerName}'.")
    {
        BlobClientName = blobClientName ?? throw new ArgumentNullException(nameof(blobClientName));
        BlobContainerName = blobContainerName ?? throw new ArgumentNullException(nameof(blobContainerName));
    }
}
