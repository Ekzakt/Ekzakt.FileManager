namespace Ekzakt.FileManager.Core.Options;

#nullable disable

public class FileManagerOptions
{
    public const string SectionName = "Ekzakt:FileManager";

    public string BaseLocation { get; init; }

    public UploadOptions Upload { get; init; } = new();
}
