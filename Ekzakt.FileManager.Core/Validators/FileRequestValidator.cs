using Ekzakt.FileManager.Core.Models;
using Regexes = Ekzakt.Utilities.Validation.Regex;
using FluentValidation;

namespace Ekzakt.FileManager.Core.Validators;

public class FileRequestValidator : AbstractValidator<SaveFileRequest>
{
    public FileRequestValidator()
    {
        RuleFor(x => x.ContainerName)
            .NotNull()
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.CONTAINERNAME);

        RuleFor(x => x.FileName)
            .NotNull()
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.BLOB_CLIENT_NAME);

        RuleFor(x => x.InputStream)
            .NotNull();
    }
}
