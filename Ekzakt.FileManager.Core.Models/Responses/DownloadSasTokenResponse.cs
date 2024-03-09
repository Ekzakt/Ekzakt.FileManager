namespace Ekzakt.FileManager.Core.Models.Responses;

public class DownloadSasTokenResponse
{
    public Uri? DownloadUri { get; set; }

    public DateTimeOffset? ExpiresOn { get; set; }

}
