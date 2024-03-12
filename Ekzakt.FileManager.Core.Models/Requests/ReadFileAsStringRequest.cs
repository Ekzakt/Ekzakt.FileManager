namespace Ekzakt.FileManager.Core.Models.Requests;

public class ReadFileAsStringRequest : AbstractFileRequest
{
    public string FileName { get; set; } = string.Empty;
}
