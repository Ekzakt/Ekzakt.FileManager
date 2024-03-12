namespace Ekzakt.FileManager.Core.Models.Requests;

public class ReadFileAsStringRequest : AbstractFileRequest
{
    public string[] Prefixes { get; set; } = new string[] { };

    public string FileName { get; set; } = string.Empty;


    public override string ToString()
    {
        var output = Path.Combine(Prefixes);
        output = Path.Combine(output, FileName);

        return output;
    }
}
