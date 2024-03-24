using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;

namespace Ekzakt.FileManager.Core.Validators;

public sealed class ReadFileAsStringRequestValidator : AbstractValidator<ReadFileAsStringRequest>
{
    public ReadFileAsStringRequestValidator()
    {
        RuleFor(x => x)
            .SetValidator(new AbstractFileRequestValidator());

        RuleFor(x => x.FileName)
            .NotNull()
            .NotEmpty();
    }
}