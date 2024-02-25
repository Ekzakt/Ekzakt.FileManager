namespace Ekzakt.FileManager.Core.Options;

public class UploadOptions
{
    public long InitialTransferSize { get; init; } = 4 * 1024 * 1024;

    public long MaximumTransferSize { get; init; } = 4 * 1024 * 1024;
}
