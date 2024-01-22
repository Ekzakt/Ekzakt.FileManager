using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.AzureBlob.Configuration;

public class AzureFileManagerOptions : IFileManagerOptions
{
    public const string OptionsName = "Ekzakt:FileManager";


    public AzureOptions Azure { get; init; } = new();

    public string TestKey { get; init; } = string.Empty;
}
