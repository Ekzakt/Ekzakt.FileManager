using Ekzakt.FileManager.Core.Models;

namespace Ekzakt.FileManager.Core.Contracts;

public interface IFileManager
{
    event EventHandler ProgressEventHandler;

    Task<SaveFileResponse> SaveAsync(string folderOrContainerName, Stream inputStream, string fileName, string? contentType = null);
}
