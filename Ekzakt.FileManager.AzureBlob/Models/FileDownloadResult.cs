using Ekzakt.FileManager.Core.Contracts;

namespace Ekzakt.FileManager.AzureBlob.Models;

internal class FileDownloadResult : IFileResult
{
    public bool IsSuccess => false;
}
