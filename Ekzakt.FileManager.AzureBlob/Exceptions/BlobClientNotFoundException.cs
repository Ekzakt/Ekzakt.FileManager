namespace Ekzakt.FileManager.AzureBlob.Exceptions;

public class BlobClientNotFoundException : Exception
{
    public string BlobClientName { get; }
    public string BlobContainerName { get; }
    public string? Path { get; }

    public BlobClientNotFoundException(string blobContainerName, string blobClientName, string? path)
        : base($"Blob in folder '{path}' with clientname '{blobClientName}' does not exist in container '{blobContainerName}'.")
    {
        BlobClientName = blobClientName ?? throw new ArgumentNullException(nameof(blobClientName));
        BlobContainerName = blobContainerName ?? throw new ArgumentNullException(nameof(blobContainerName));
        Path = path;
    }
}
