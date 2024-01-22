using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.Core.Models;

public class DeleteFileResponse : IFileResponse
{
    public bool IsSuccess => true;
}
