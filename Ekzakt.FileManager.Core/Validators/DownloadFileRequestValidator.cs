using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;

namespace Ekzakt.FileManager.Core.Validators;

public class DownloadFileRequestValidator : AbstractValidator<DownloadFileRequest>
{
    public DownloadFileRequestValidator()
    { 
        // TODO: Make this simpler!
        RuleFor(request => request.ContainerName).NotEmpty().WithMessage("Container name cannot be empty.");
        RuleFor(request => request.BlobName).NotEmpty().WithMessage("Blob name cannot be empty.");
    }
}