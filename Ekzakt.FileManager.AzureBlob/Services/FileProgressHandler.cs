using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;

namespace Ekzakt.FileManager.AzureBlob.Services;

public class FileProgressHandler : IProgress<long>
{
    private readonly AbstractSaveFileRequest _saveFileRequest;

    public FileProgressHandler(AbstractSaveFileRequest saveFileRequest)
    {
        _saveFileRequest = saveFileRequest;
    }


    public void Report(long bytesSent)
    {
        if (_saveFileRequest is null)
        {
            return;
        }

        var args = new ProgressEventArgs
        {
            FileName = _saveFileRequest.FileName,
            FileSize = _saveFileRequest.InitialFileSize,
            BytesSent = bytesSent,
        };

        _saveFileRequest.ProgressHandler?.Report(args);
    }
}
