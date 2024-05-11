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

internal class SaveFileOperation : AbstractBunnyFileOperation<SaveFileOperation>, IFileOperation<SaveFileRequest, string?>
{
    private readonly ILogger<SaveFileOperation> _logger;
    private readonly EkzaktFileManagerBunnyOptions _options;
    private readonly BunnyHttpClient _httpClient;
    private readonly SaveFileRequestValidator _validator;

    public SaveFileOperation(
        ILogger<SaveFileOperation> logger, 
        IOptions<EkzaktFileManagerBunnyOptions> options,
        BunnyHttpClient httpClient,
        SaveFileRequestValidator validator) : base(logger, options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<FileResponse<string?>> ExecuteAsync(SaveFileRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _validator, out FileResponse<string?> validationResponse, _options))
        {
            return validationResponse!;
        }

        try
        {
            _logger.LogRequestStarted(request);

            request!.FileStream!.Position = 0;

            request!.Paths.Add(request!.FileName);

            var uri = GetBunnyUri([.. request.Paths]);

            using (var content = new StreamContent(request!.FileStream!))
            {
                var message = new HttpRequestMessage(HttpMethod.Put, uri)
                {
                    Content = content
                };

                var response = await _httpClient.Client.SendAsync(message);

                if (response.IsSuccessStatusCode)
                {
                    return new FileResponse<string?>
                    {
                        Status = HttpStatusCode.Created,
                        Message = "File created successfully."
                    };
                }

                return new FileResponse<string?>
                {
                    Status = response.StatusCode,
                    Message = "The specified could not be saved."
                };

            }
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while saving the file {FileName} in container {StorageZone}. Exception {Exception}", request!.FileName, request!.BaseLocation, ex);
            return new FileResponse<string?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "File could not be saved."
            };
        }
        finally
        {
            request!.FileStream?.Close();
            request!.FileStream?.Dispose();
        }
    }
}
