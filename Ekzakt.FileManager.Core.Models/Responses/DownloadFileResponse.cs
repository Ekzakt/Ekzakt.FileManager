namespace Ekzakt.FileManager.Core.Models.Responses;

public class DownloadFileResponse
{
    public Uri? DownloadUri { get; set; }

    public DateTimeOffset? ExpiresOn { get; set; }

}
