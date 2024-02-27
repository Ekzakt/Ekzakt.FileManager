namespace Ekzakt.FileManager.Core.Options;

public class FileManagerOptions
{
    public const string SectionName = "Ekzakt:FileManager";

    public string[] ContainerNames { get; set; } = [];

    public UploadOptions Upload { get; set; } = new();
}
