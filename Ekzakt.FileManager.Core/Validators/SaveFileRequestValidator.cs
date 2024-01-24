using Ekzakt.FileManager.Core.Models;
using FluentValidation;

namespace Ekzakt.FileManager.Core.Validators;

public class SaveFileRequestValidator : AbstractValidator<SaveFileRequest>
{
    public SaveFileRequestValidator()
    {
        // TODO: Make rules for all properties!
        RuleFor(x => x.FileName)
            .NotNull()
            .NotEmpty();

    }
}
