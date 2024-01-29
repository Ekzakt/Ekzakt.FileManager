namespace Ekzakt.FileManager.Core.Contracts;

public abstract class AbstractFileRequest
{
    public Guid CorrelationId { get; set; }

    public string ContainerName { get; set; } = string.Empty;
}
