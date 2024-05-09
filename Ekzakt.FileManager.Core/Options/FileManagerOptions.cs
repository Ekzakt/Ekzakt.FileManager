namespace Ekzakt.FileManager.Core.Options;

#nullable disable

[Obsolete("This class has been renamed to EkzaktFileManagerAzureOptions and moved to namespace Ekzakt.FileManager.AzureBlob.Configuration")]
public class FileManagerOptions
{
    public const string SectionName = "Ekzakt:FileManager";

    public string BaseLocation { get; init; }

    public UploadOptions Upload { get; init; } = new();
}
