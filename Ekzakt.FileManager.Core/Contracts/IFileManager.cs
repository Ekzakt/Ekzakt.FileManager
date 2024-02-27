using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;

namespace Ekzakt.FileManager.Core.Contracts;

public interface IFileManager
{
    Task<FileResponse<string?>> SaveFileAsync<T>(T saveFileRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest;

    Task<FileResponse<string?>> DeleteFileAsync<T>(T deleteFileRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest;

    Task<FileResponse<IEnumerable<FileInformation>?>> ListFilesAsync<T>(T listFilesRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest;

    Task<FileResponse<DownloadFileResponse?>> DownloadFileAsync<T>(T downloadFileRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest;

    Task<FileResponse<string?>> SaveFileChunkedAsync<T>(T saveFileRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest;
}
