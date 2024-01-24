using Ekzakt.FileManager.Core.Models;

namespace Ekzakt.FileManager.AzureBlob.Services;

public class FileProgressHandler : IProgress<long>
{
    private readonly SaveFileRequest _saveFileRequest;
    private int _progressFlag = 0;

    public FileProgressHandler(SaveFileRequest saveFileRequest)
    {
        _saveFileRequest = saveFileRequest;    
    }


    public void Report(long bytesSend)
    {
        var args = new ProgressEventArgs
        {
            FileName = _saveFileRequest.FileName,
            FileSize = _saveFileRequest.FileLangth,
            BytesSent = bytesSend,
        };

        _saveFileRequest.ProgressHandler?.Report(args);

        //if (_progressFlag != (int)args.PercentageDone) {
        //    _progressFlag = (int)args.PercentageDone;
        //    _saveFileRequest.ProgressHandler?.Report(args);
        //}
    }
}
