using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;

namespace Ekzakt.FileManager.Core.Contracts;

[Obsolete("Use IEkzaktFileManager interface instead. This interface will be deleted in a future version.")]
public interface IFileManager
{
    Task<FileResponse<string?>> SaveFileAsync<TRequest>(TRequest saveFileRequest, CancellationToken cancellationToken = default)
        where TRequest : AbstractFileRequest;

    Task<FileResponse<string?>> DeleteFileAsync<TRequest>(TRequest deleteFileRequest, CancellationToken cancellationToken = default)
        where TRequest : AbstractFileRequest;

    Task<FileResponse<IEnumerable<FileProperties>?>> ListFilesAsync<TRequest>(TRequest listFilesRequest, CancellationToken cancellationToken = default)
        where TRequest : AbstractFileRequest;

    Task<FileResponse<DownloadSasTokenResponse?>> DownloadSasTokenAsync<TRequest>(TRequest downloadFileRequest, CancellationToken cancellationToken = default)
        where TRequest : AbstractFileRequest;

    Task<FileResponse<string?>> SaveFileChunkedAsync<TRequest>(TRequest saveFileRequest, CancellationToken cancellationToken = default)
        where TRequest : AbstractFileRequest;

    Task<FileResponse<string?>> ReadFileStringAsync<TRequest>(TRequest readFileRequest, CancellationToken cancellationToken = default)
        where TRequest : AbstractFileRequest;
}
