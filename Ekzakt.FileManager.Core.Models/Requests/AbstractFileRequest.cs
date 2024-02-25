namespace Ekzakt.FileManager.Core.Models.Requests;

public abstract class AbstractFileRequest
{
    public string ContainerName { get; set; } = string.Empty;
}
