using Ekzakt.FileManager.Bunny.Configuration;
using Ekzakt.FileManager.Bunny.HttpClients;
using Ekzakt.FileManager.Bunny.Models;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Extensions;
using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace Ekzakt.FileManager.Bunny.Operations;

internal class ListFilesOperation : AbstractBunnyFileOperation<ListFilesOperation>, IFileOperation<ListFilesRequest, IEnumerable<FileInformation>?>
{
    private readonly ILogger<ListFilesOperation> _logger;
    private EkzaktFileManagerBunnyOptions _options;
    private readonly BunnyHttpClient _httpClient;
    private readonly ListFilesRequestValidator _listFilesValidator;

    public ListFilesOperation(
        ILogger<ListFilesOperation> logger,
        IOptions<EkzaktFileManagerBunnyOptions> options,
        BunnyHttpClient httpClient,
        ListFilesRequestValidator listFilesValidator) : base(logger, options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _listFilesValidator = listFilesValidator ?? throw new ArgumentNullException(nameof(listFilesValidator));
    }


    public async Task<FileResponse<IEnumerable<FileInformation>?>> ExecuteAsync(ListFilesRequest request, CancellationToken cancellationToken = default)
    {
        if (!ValidateRequest(request!, _listFilesValidator, out FileResponse<IEnumerable<FileInformation>?> validationResponse, _options))
        {
            return validationResponse;
        }

        try
        {
            _logger.LogRequestStarted(request);

            List<FileInformation> filesList = [];

            var uri = GetBunnyUri([.. request.Paths]);

            var response = await _httpClient.Client.GetAsync(uri);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<BunnyFileInformation>>(responseJson);
                
                if (result is not null && result.Count > 0)
                {
                    foreach(var file in result)
                    {
                        filesList.Add(new FileInformation
                        {
                            Name = file.ObjectName,
                            Size = file.Length,
                            CreatedOn = file.DateCreated
                        });
                    }
                }
            }

            return new FileResponse<IEnumerable<FileInformation>?>
            {
                Status = filesList.Count > 0 ? HttpStatusCode.OK : HttpStatusCode.NoContent,
                Message = filesList.Count > 0 ? "File list retrieved successfully." : "No files where found.",
                Data = filesList.OrderByDescending(x => x.CreatedOn).AsEnumerable()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("An error occured while retreiving filelist from blob container {BaseLocation}. Exception {Exception}", request!.BaseLocation, ex);

            return new FileResponse<IEnumerable<FileInformation>?>
            {
                Status = HttpStatusCode.InternalServerError,
                Message = "File list could not be retrieved."
            };
        }
    }
}
