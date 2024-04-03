using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Models;

namespace Ekzakt.FileManager.Core.Contracts;

public interface IEkzaktFileManager
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
