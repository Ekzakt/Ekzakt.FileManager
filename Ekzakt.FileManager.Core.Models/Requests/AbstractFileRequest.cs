namespace Ekzakt.FileManager.Core.Models.Requests;

public abstract class AbstractFileRequest
{
    [Obsolete("Use BlobContainerName instead.")]
    public string ContainerName { get; set; } = string.Empty;

    public string BlobContainerName { get; set; } = string.Empty;
}
