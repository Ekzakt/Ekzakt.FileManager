using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.Core.Models;

public class SaveFileResponse : IFileResponse
{
    public string? RawResult { get; set; }
    public bool IsSuccess => true;
}
