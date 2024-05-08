using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;
using Regexes = Ekzakt.Utilities.Validation.Regex;


namespace Ekzakt.FileManager.Core.Validators;

public sealed class ListFilesRequestValidator : AbstractValidator<ListFilesRequest>
{
    public ListFilesRequestValidator()
    {
        RuleFor(x => x)
            .SetValidator(new AbstractFileRequestValidator());
    }
}
