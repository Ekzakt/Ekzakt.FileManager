using Ekzakt.FileManager.Bunny.Configuration;
using Ekzakt.FileManager.Bunny.HttpClients;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace Ekzakt.FileManager.Bunny.Operations;

internal class DeleteFileOperation : AbstractBunnyFileOperation<DeleteFileOperation>, IFileOperation<DeleteFileRequest, string?>
{
    private readonly ILogger<DeleteFileOperation> _logger;
    private EkzaktFileManagerBunnyOptions _options;
    private readonly BunnyHttpClient _httpClient;
    private DeleteFileRequestValidator _validator;

    public DeleteFileOperation(
        ILogger<DeleteFileOperation> logger, 
        IOptions<EkzaktFileManagerBunnyOptions> options,
        BunnyHttpClient httpClient,
        DeleteFileRequestValidator validator) : base(logger, options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));    
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<FileResponse<string?>> ExecuteAsync(DeleteFileRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _validator, out FileResponse<string?> validationResponse, _options))
        {
            return validationResponse;
        }

        try
        {
            _logger.LogRequestStarted(request);

            request.Paths.Add(request!.FileName);

            var uri = GetBunnyUri([.. request.Paths]);

            var response = await _httpClient.Client.DeleteAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                return new FileResponse<string?>
                {
                    Status = HttpStatusCode.OK,
                    Message = "The file was deleted succuessfully."
                };
            }

            return new FileResponse<string?>
            {
                Status = response.StatusCode,
                Message = "The specified file could not be deleted."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while deleteing the file {FileName}. Exception {Exception}", request!.FileName, ex);

            return new FileResponse<string?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "The file could not be deleted."
            };
        }
    }
}
