namespace Ekzakt.FileManager.AzureBlob.Services;

public class ProgressEventArgs : EventArgs
{
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public long? BytesSent { get; set; }
}
