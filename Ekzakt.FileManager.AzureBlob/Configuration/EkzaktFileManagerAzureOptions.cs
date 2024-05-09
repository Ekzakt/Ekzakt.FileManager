namespace Ekzakt.FileManager.AzureBlob.Configuration;

#nullable disable

public class EkzaktFileManagerAzureOptions
{
    public const string SectionName = "Ekzakt:FileManager:Azure";

    public string BaseLocation { get; init; }

    public UploadOptions Upload { get; init; } = new();
}
