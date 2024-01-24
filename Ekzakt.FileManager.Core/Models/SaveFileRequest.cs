
namespace Ekzakt.FileManager.Core.Models;

public class SaveFileRequest
{
    public string ContainerName { get; set; } = string.Empty;


    public string FileName { get; set; } = string.Empty;


    public Stream? InputStream { get; private set; }

    
    public string? ContentType { get; set; }


    public IProgress<ProgressEventArgs>? ProgressHandler { get; private set; }


    public bool ThrowOnError { get => false; }


    public long FileLangth { get => InputStream?.Length ?? 0; }


    public SaveFileRequest SetInputStream(Stream? inputStream)
    {
        if (inputStream is not null)
        {
            InputStream = inputStream;
            InputStream.Position = 0;
        }

        return this;
    }


    public SaveFileRequest SetProgressHandler(IProgress<ProgressEventArgs>? progressHandler)
    {
        ProgressHandler ??= progressHandler;

        return this;
    }
}
