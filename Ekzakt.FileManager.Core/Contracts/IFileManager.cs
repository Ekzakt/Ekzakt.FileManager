using Ekzakt.FileManager.Core.Models.Responses;

namespace Ekzakt.FileManager.Core.Contracts;

public interface IFileManager
{
    Task<FileResponse<string?>> SaveFileAsync<T>(T saveFileRequest, CancellationToken cancellationToken = default) where T : AbstractFileRequest;


    //Task<FileResponse<string?>> DeleteFileAsync<T>(T request) where T : AbstractFileRequest;

    //Task<FileResponse<IEnumerable<string>>> ListFilesAsync<T>(T request) where T : AbstractFileRequest;

    //Task<FileResponse<string>> DownloadFileAsync<T>(T request) where T : AbstractFileRequest;
}
