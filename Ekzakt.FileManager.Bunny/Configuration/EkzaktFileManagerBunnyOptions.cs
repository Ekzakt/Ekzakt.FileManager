using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.Bunny.Configuration;

#nullable disable

public class EkzaktFileManagerBunnyOptions : IEkzaktFileManagerOptions
{
    public const string SectionName = "Ekzakt:FileManager:Bunny";

    public string ApiKey { get; set; }

    public string MainReplicationRegion { get; set; }

    public string BaseStorageZoneName { get; init; }

    public string BaseLocation { get; init; }

    public int HttpTimeout { get; init; } = 60;

    public int[] RetryPolicy { get; init; } = [];

    public List<StorageZone> StorageZones { get; init; } = [];
}
