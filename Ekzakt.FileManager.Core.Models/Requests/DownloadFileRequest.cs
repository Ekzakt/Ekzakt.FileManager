namespace Ekzakt.FileManager.Core.Models.Requests;

public class DownloadFileRequest : AbstractFileRequest
{
    public string FileName { get; set; } = string.Empty;
}
