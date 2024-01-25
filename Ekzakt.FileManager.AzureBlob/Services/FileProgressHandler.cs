using Ekzakt.FileManager.Core.Models;

namespace Ekzakt.FileManager.AzureBlob.Services;

public class FileProgressHandler : IProgress<long>
{
    private readonly SaveFileRequest _saveFileRequest;

    public FileProgressHandler(SaveFileRequest saveFileRequest)
    {
        _saveFileRequest = saveFileRequest;    
    }


    public void Report(long bytesSend)
    {
        var args = new ProgressEventArgs
        {
            FileName = _saveFileRequest.FileName,
            FileSize = _saveFileRequest.FileLength,
            BytesSent = bytesSend,
        };

        _saveFileRequest.ProgressHandler?.Report(args);
    }
}
