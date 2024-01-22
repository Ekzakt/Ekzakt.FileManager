using Ekzakt.FileManager.AzureBlob.Configuration;
using Ekzakt.FileManager.AzureBlob.Models;
using Ekzakt.FileManager.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ekzakt.FileManager.AzureBlob.Services;

public class AzureBlobFileManager(
    ILogger<AzureBlobFileManager> _logger,
    IOptions<AzureFileManagerOptions> _options
    ) : IFileManager
{
    public async Task<IFileResult> SaveAsync()
    {
        var x = _options;

        await Task.Delay(1);

        return new FileSaveResult();
    }
}
