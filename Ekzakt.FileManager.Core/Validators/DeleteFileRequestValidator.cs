using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;

namespace Ekzakt.FileManager.Core.Validators;

public sealed class DeleteFileRequestValidator : AbstractValidator<DeleteFileRequest>
{
    public DeleteFileRequestValidator()
    {
        RuleFor(x => x)
            .SetValidator(new AbstractFileRequestValidator());

        RuleFor(request => request.FileName)
            .NotEmpty()
            .WithMessage("Blob name cannot be empty.");
    }
}