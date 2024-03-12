using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;
using Regexes = Ekzakt.Utilities.Validation.Regex;


namespace Ekzakt.FileManager.Core.Validators;

public class ListFilesRequestValidator : AbstractValidator<ListFilesRequest>
{
    public ListFilesRequestValidator()
    {
        RuleFor(x => x.BaseLocation)
            .NotNull()
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.BLOB_CONTAINER_NAME);
    }
}
