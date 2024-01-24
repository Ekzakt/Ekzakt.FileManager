using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.AzureBlob.Models;

public class FileSaveResult : IFileResult
{
    public bool IsSuccess => true;
}
