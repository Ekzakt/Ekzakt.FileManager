using Regexes = Ekzakt.Utilities.Validation.Regex;
using FluentValidation;
using Ekzakt.FileManager.Core.Options;

namespace Ekzakt.FileManager.AzureBlob.Validators;

public class FileManagerOptionValidator : AbstractValidator<FileManagerOptions>
{
    public FileManagerOptionValidator()
    {
        RuleForEach(x => x.ContainerNames)
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.BLOB_CONTAINER_NAME);
    }
}
