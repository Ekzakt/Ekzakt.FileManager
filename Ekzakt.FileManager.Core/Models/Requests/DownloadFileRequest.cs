using Ekzakt.FileManager.Core.Contracts;
using Microsoft.Identity.Client;

namespace Ekzakt.FileManager.Core.Models.Requests;

public class DownloadFileRequest : AbstractFileRequest
{
    public string FileName { get; set; } = string.Empty;
}
