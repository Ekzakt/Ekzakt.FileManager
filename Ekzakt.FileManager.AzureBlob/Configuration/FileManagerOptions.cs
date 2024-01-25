using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.AzureBlob.Configuration;

public class FileManagerOptions : IFileManagerOptions
{
    public const string SectionName = "Ekzakt:FileManager:Azure:Storage";

    public string[] ContainerNames { get; set; } = [];
}
