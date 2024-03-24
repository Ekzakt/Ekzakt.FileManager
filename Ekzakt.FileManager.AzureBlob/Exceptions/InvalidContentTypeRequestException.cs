namespace Ekzakt.FileManager.AzureBlob.Exceptions;

public class InvalidContentTypeRequestException : Exception
{
    public string? Prefix { get; }

    public string BlobContainerName { get; }

    public Type Type { get; }


    public InvalidContentTypeRequestException(string blobContainerName, string? prefix, Type type)
        : base($"Invalid request type '{nameof(type.GetType)}'. Currently only type {nameof(string.GetType)} is supported.")
    {
        BlobContainerName = blobContainerName;
        Prefix = prefix;
        Type = type;
    }
}
