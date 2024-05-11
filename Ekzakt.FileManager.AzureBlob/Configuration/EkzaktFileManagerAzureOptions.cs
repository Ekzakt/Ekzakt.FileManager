using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.AzureBlob.Configuration;

#nullable disable

public class EkzaktFileManagerAzureOptions : IEkzaktFileManagerOptions
{
    public const string SectionName = "Ekzakt:FileManager:Azure";

    public string BaseLocation { get; init; }

    public UploadOptions Upload { get; init; } = new();
}
