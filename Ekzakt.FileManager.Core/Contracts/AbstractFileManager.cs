using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using FluentValidation;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Ekzakt.FileManager.Core.Contracts;

public abstract class AbstractFileManager
{
    private readonly ILogger<AbstractFileManager> _logger;

    public AbstractFileManager(ILogger<AbstractFileManager> logger)
    {
        _logger = logger;
    }

    public bool ValidateRequest<TRequest, TValidator, TResponse>(TRequest request, TValidator validator, out FileResponse<TResponse?> response)
        where TRequest : AbstractFileRequest
        where TValidator : AbstractValidator<TRequest>
        where TResponse : class
    {
        var validationResult = validator.Validate(request);

        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));

            _logger.LogWarning("{requestName} validation failed. CorrelationId: {correlationId}, Error: {errorMessage}",
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
