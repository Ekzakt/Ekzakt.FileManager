namespace Ekzakt.FileManager.AzureBlob.Configuration;

public class StorageAccountOptions
{
    public const string OptionsName = "Azure:StorageAccount";

    public string Name { get; set; } = string.Empty;
    public string[] ContainerNames { get; set; } = [];
}

