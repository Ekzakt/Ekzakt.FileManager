using Ekzakt.FileManager.Core.Models;

namespace Ekzakt.FileManager.Core.Contracts;

public interface IFileManager
{
    Task<SaveFileResponse> SaveAsync(SaveFileRequest saveFileRequest, CancellationToken cancellationToken = default);
}
