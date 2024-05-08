namespace Ekzakt.FileManager.Core.Options;

public class FileManagerOptions
{
    public const string SectionName = "Ekzakt:FileManager";

    public string BaseLocation { get; set; } = string.Empty;

    public UploadOptions Upload { get; set; } = new();
}
