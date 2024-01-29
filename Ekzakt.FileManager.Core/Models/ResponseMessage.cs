namespace Ekzakt.FileManager.Core.Models;

public class ResponseMessage
{
    public ResponseMessage(params object[]? args)
    {
        Args = args;
    }

    public object[]? Args { get; private set; }
    public string Log { get; set; } = string.Empty;
    public string Return { get; set;} = string.Empty;

}
