namespace Ekzakt.FileManager.Core.Models;

/// <summary>
/// This class is used to parse the json content
/// from a MultipartFormDataContent stream.
/// </summary>
public class SaveFileFormDisposition
{
    public string Id { get; set; } = string.Empty;

    public string FileContentType { get; set; } = string.Empty;

    public long InitialFileSize { get; set; } = 0;
}
