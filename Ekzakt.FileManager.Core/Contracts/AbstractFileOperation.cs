using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ekzakt.FileManager.Core.Contracts;

public abstract class AbstractFileOperation<TLogger>
    where TLogger : class
{
    private readonly ILogger<TLogger> _logger;

    protected AbstractFileOperation(
        ILogger<TLogger> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public bool ValidateRequest<TRequest, TValidator, TResponse>(TRequest request, TValidator validator, out FileResponse<TResponse?> response, IEkzaktFileManagerOptions options)
        where TRequest : AbstractFileRequest
        where TValidator : AbstractValidator<TRequest>
        where TResponse : class
    {
        if (string.IsNullOrEmpty(request.BaseLocation))
        {
            request.BaseLocation = options.BaseLocation;
        }

        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));

            _logger.LogWarning("{requestName} validation failed. Error: {errorMessage}",
                typeof(TRequest).Name,
                errorMessage
            );

            response = new FileResponse<TResponse?>
            {
                Status = HttpStatusCode.BadRequest,
                Message = errorMessage,
            };

            return false;
        }

        response = new();

        return true;
    }
}
