using System.Text.Json.Serialization;

namespace Ekzakt.FileManager.Core.Models.Requests;

public class SaveFileRequest : AbstractSaveFileRequest
{
    [JsonIgnore]
    public Stream? FileStream;

    public override long ContentLength => FileStream?.Length ?? 0;

    public override long InitialFileSize { get; set; } = 0;

}
