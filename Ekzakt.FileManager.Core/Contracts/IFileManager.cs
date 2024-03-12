using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;

namespace Ekzakt.FileManager.Core.Contracts;

public interface IFileManager
{
    Task<FileResponse<string?>> SaveFileAsync<TRequest>(TRequest saveFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest;

    Task<FileResponse<string?>> DeleteFileAsync<TRequest>(TRequest deleteFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest;

    Task<FileResponse<IEnumerable<Models.FileInformation>?>> ListFilesAsync<TRequest>(TRequest listFilesRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest;

    Task<FileResponse<DownloadSasTokenResponse?>> DownloadSasTokenAsync<TRequest>(TRequest downloadFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest;

    Task<FileResponse<string?>> SaveFileChunkedAsync<TRequest>(TRequest saveFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest;

    Task<FileResponse<string?>> ReadFileStringAsync<TRequest>(TRequest readFileRequest, CancellationToken cancellationToken = default) where TRequest : AbstractFileRequest;
}
