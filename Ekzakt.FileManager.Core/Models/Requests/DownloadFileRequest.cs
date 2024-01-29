using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.Core.Models.Requests;

public class DownloadFileRequest : AbstractFileRequest
{
    public string BlobName { get; set; } = string.Empty;
}
