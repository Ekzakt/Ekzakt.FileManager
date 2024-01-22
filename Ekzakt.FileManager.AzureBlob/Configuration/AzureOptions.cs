namespace Ekzakt.FileManager.AzureBlob.Configuration;

public class AzureOptions
{
    public const string OptionsName = "Azure";

    public StorageAccountOptions StorageAccount { get; init; } = new();
}
