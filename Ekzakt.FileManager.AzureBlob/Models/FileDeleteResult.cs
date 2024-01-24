using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.AzureBlob.Models;

public class FileDeleteResult : IFileResult
{
    public bool IsSuccess => true;
}
