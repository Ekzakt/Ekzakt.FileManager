using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;

namespace Ekzakt.FileManager.Bunny.Services;

public class EkzaktFileManagerBunny : IEkzaktFileManager
{
    private readonly IFileOperation<ListFilesRequest, IEnumerable<FileInformation>?> _listFilesOperation;
    private readonly IFileOperation<SaveFileRequest, string?> _saveFileOperation;
    private readonly IFileOperation<SaveFileChunkedRequest, string?> _saveFileChunkedOperation;
    private readonly IFileOperation<DeleteFileRequest, string?> _deleteFileOperation;
    private readonly IFileOperation<DownloadSasTokenRequest, DownloadSasTokenResponse?> _downloadFileOperation;
    private readonly IFileOperation<ReadFileAsStringRequest, string?> _readFileAsStringOperation;

    public EkzaktFileManagerBunny(
        IFileOperation<ListFilesRequest, IEnumerable<FileInformation>?> listFilesOperation,
        IFileOperation<SaveFileRequest, string?> saveFileOperation,
        IFileOperation<SaveFileChunkedRequest, string?> saveFileChunkedOperation,
        IFileOperation<DeleteFileRequest, string?> deleteFileOperation,
        IFileOperation<DownloadSasTokenRequest, DownloadSasTokenResponse?> downloadFileOperation,
        IFileOperation<ReadFileAsStringRequest, string?> readFileAsStringOperation)
    {
        _listFilesOperation = listFilesOperation;
        _saveFileOperation = saveFileOperation;
        _saveFileChunkedOperation = saveFileChunkedOperation;
        _deleteFileOperation = deleteFileOperation;
        _downloadFileOperation = downloadFileOperation;
        _readFileAsStringOperation = readFileAsStringOperation;
    }

    public Task<FileResponse<string?>> DeleteFileAsync<TRequest>(TRequest deleteFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest
    {
        throw new NotImplementedException();
    }

    public Task<FileResponse<DownloadSasTokenResponse?>> DownloadSasTokenAsync<TRequest>(TRequest downloadFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest
    {
        throw new NotImplementedException();
    }

    public Task<FileResponse<IEnumerable<FileInformation>?>> ListFilesAsync<TRequest>(TRequest listFilesRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest
    {
        throw new NotImplementedException();
    }

    public Task<FileResponse<string?>> ReadFileStringAsync<TRequest>(TRequest readFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest
    {
        throw new NotImplementedException();
    }

    public Task<FileResponse<string?>> SaveFileAsync<TRequest>(TRequest saveFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest
    {
        throw new NotImplementedException();
    }

    public Task<FileResponse<string?>> SaveFileChunkedAsync<TRequest>(TRequest saveFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest
    {
        throw new NotImplementedException();
    }
}
