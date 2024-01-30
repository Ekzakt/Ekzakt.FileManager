using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.Core.Models.Requests;

public class SaveFileRequest : AbstractFileRequest
{
    public string FileName { get; set; } = string.Empty;

    [NonSerialized]
    public FileStream? FileStream;

    [NonSerialized]
    public IProgress<ProgressEventArgs>? ProgressHandler;

    public long FileLength => FileStream?.Length ?? 0;
}
