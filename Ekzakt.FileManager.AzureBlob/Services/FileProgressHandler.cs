using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;

namespace Ekzakt.FileManager.AzureBlob.Services;

public class FileProgressHandler : IProgress<long>
{
    private readonly SaveFileRequest _saveFileRequest;

    public FileProgressHandler(SaveFileRequest saveFileRequest)
    {
        _saveFileRequest = saveFileRequest;    
    }


    public void Report(long bytesSent)
    {
        var args = new ProgressEventArgs
        {
            FileName = _saveFileRequest.FileName,
            FileSize = _saveFileRequest.FileLength,
            BytesSent = bytesSent,
        };

        _saveFileRequest.ProgressHandler?.Report(args);
    }
}
