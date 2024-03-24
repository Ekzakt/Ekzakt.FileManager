using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;
using Regexes = Ekzakt.Utilities.Validation.Regex;

namespace Ekzakt.FileManager.Core.Validators;

public sealed class SaveFileRequestValidator : AbstractValidator<SaveFileRequest>
{
    public SaveFileRequestValidator()
    {
        RuleFor(x => x.BaseLocation)
            .NotNull()
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.BLOB_CONTAINER_NAME);

        RuleFor(x => x.FileName)
            .NotNull()
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.BLOB_CLIENT_NAME);

        RuleFor(x => x.InitialFileSize)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.FileStream)
            .NotNull()
            .NotEmpty();
    }
}
