using Ekzakt.FileManager.AzureBlob.Configuration;
using Regexes = Ekzakt.Utilities.Validation.Regex;
using FluentValidation;

namespace Ekzakt.FileManager.AzureBlob.Validators;

public class AzureContainerValidator : AbstractValidator<FileManagerOptions>
{
    public AzureContainerValidator()
    {
        RuleForEach(x => x.ContainerNames)
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.CONTAINERNAME);
    }
}
