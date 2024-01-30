using Ekzakt.FileManager.AzureBlob.Configuration;
using Regexes = Ekzakt.Utilities.Validation.Regex;
using FluentValidation;

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
