using System.Text.Json.Serialization;

namespace Ekzakt.FileManager.Core.Models.Requests;

public class SaveChunkedFileRequest : AbstractSaveFileRequest
{
    [JsonIgnore]
    public byte[] ChunkData { get; set; } = new byte[0];

    public int ChunkIndex { get; set; }

    public long ChunkTreshold { get; set; } = 1024 * 1024;

    public long ChunkSize => ChunkData?.Length ?? 0;

    public bool Commit => ChunkTreshold * (ChunkIndex + 1) >= InitialFileSize;

    public override long InitialFileSize { get; set ; }

    public override long ContentLength => InitialFileSize;
}
