namespace Ekzakt.FileManager.AzureBlob.Exceptions;

public class BlobsNotFoundException : Exception
{
    public string? Prefix { get; }

    public string BlobContainerName { get; }

    public BlobsNotFoundException(string blobContainerName, string? prefix)
        : base($"No blobs with prefix '{prefix}' where found in container {blobContainerName}.")
    {
        BlobContainerName = blobContainerName;
        Prefix = prefix;
    }
}
