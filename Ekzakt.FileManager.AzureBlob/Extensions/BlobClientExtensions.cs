using Azure.Storage.Blobs;

namespace Ekzakt.FileManager.AzureBlob.Extensions;

public static class BlobClientExtensions
{
    public static async Task<string?> ReadAsStringAsync(this BlobClient blobClient)
    {
        using var stream = new MemoryStream();

        await blobClient.DownloadToAsync(stream);

        stream.Position = 0;

        using var streamReader = new StreamReader(stream);

        var result = await streamReader.ReadToEndAsync();

        return result;
    }
}
