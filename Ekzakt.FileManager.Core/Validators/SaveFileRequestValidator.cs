using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;
using Regexes = Ekzakt.Utilities.Validation.Regex;

namespace Ekzakt.FileManager.Core.Validators;

public class SaveFileRequestValidator : AbstractValidator<SaveFileRequest>
{
    public SaveFileRequestValidator()
    {
        RuleFor(x => x.ContainerName)
            .NotNull()
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.CONTAINER_NAME);

        RuleFor(x => x.FileName)
            .NotNull()
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.BLOB_CLIENT_NAME);

        RuleFor(x => x.FileStream)
            .NotNull()
            .NotEmpty();
    }
}
