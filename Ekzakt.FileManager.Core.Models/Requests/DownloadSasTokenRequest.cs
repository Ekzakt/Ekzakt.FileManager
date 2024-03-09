namespace Ekzakt.FileManager.Core.Models.Requests;

public class DownloadSasTokenRequest : AbstractFileRequest
{
    public string FileName { get; set; } = string.Empty;
}
