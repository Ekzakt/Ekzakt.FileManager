using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;

namespace Ekzakt.FileManager.AzureBlob.Services;

public class AzureBlobFileManager : IFileManager
{
    private readonly IFileOperation<ListFilesRequest, IEnumerable<FileInformation>?> _listFilesOperation;
    private readonly IFileOperation<SaveFileRequest, string?> _saveFileOperation;
    private readonly IFileOperation<SaveFileChunkedRequest, string?> _saveFileChunkedOperation;
    private readonly IFileOperation<DeleteFileRequest, string?> _deleteFileOperation;
    private readonly IFileOperation<DownloadFileRequest, DownloadFileResponse?> _downloadFileOperation;


    public AzureBlobFileManager(
        IFileOperation<ListFilesRequest, IEnumerable<FileInformation>?> listFilesOperation,
        IFileOperation<SaveFileRequest, string?> saveFileOperation,
        IFileOperation<SaveFileChunkedRequest, string?> saveFileChunkedOperation,
        IFileOperation<DeleteFileRequest, string?> deleteFileOperation,
        IFileOperation<DownloadFileRequest, DownloadFileResponse?> downloadFileOperation)
    {
        _listFilesOperation = listFilesOperation;
        _saveFileOperation = saveFileOperation;
        _saveFileChunkedOperation = saveFileChunkedOperation;
        _deleteFileOperation = deleteFileOperation;
        _downloadFileOperation = downloadFileOperation;
    }


    public async Task<FileResponse<IEnumerable<FileInformation>?>> ListFilesAsync<T>(T listFilesRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest
    {
        var request = listFilesRequest is not null
            ? listFilesRequest as ListFilesRequest
            : new ListFilesRequest();

        return await _listFilesOperation.ExecuteAsync(request!, cancellationToken);
    }


    public async Task<FileResponse<string?>>SaveFileAsync<T>(T saveFileRequest, CancellationToken cancellationToken = default)
        where T : AbstractFileRequest
    {
        var request = saveFileRequest is not null
            ? saveFileRequest as SaveFileRequest
            : new SaveFileRequest();

        return await _saveFileOperation.ExecuteAsync(request!, cancellationToken);
    }


    public async Task<FileResponse<string?>> SaveFileChunkedAsync<T>(T saveFileRequest, CancellationToken cancellationToken = default)
        where T : AbstractFileRequest
    {
        var request = saveFileRequest is not null
            ? saveFileRequest as SaveFileChunkedRequest
            : new SaveFileChunkedRequest();

        return await _saveFileChunkedOperation.ExecuteAsync(request!, cancellationToken);
    }


    public async Task<FileResponse<string?>> DeleteFileAsync<T>(T deleteFileRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest
    {
        var request = deleteFileRequest is not null
            ? deleteFileRequest as DeleteFileRequest
            : new DeleteFileRequest();

        return await _deleteFileOperation.ExecuteAsync(request!, cancellationToken);
    }


    public async Task<FileResponse<DownloadFileResponse?>> DownloadFileAsync<T>(T downloadFileRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest
    {
        var request = downloadFileRequest is not null
            ? downloadFileRequest as DownloadFileRequest
            : new DownloadFileRequest();

        return await _downloadFileOperation.ExecuteAsync(request!, cancellationToken);
    }
}