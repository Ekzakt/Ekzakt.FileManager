namespace Ekzakt.FileManager.Core.Models.Requests;

public abstract class AbstractFileRequest
{
    public Guid CorrelationId { get; set; }

    public string ContainerName { get; set; } = string.Empty;
}
