using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.Core.Models.Requests;

public class DeleteFileRequest : AbstractFileRequest
{
    public string FileName { get; set; } = string.Empty;
}
