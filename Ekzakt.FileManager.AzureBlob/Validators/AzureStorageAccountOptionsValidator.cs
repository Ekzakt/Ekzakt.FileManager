using Ekzakt.FileManager.AzureBlob.Configuration;
using Regexes = Ekzakt.Utilities.Validation.Regex;
using FluentValidation;

namespace Ekzakt.FileManager.AzureBlob.Validators;

public class AzureStorageAccountOptionsValidator : AbstractValidator<StorageAccountOptions>
{
    public AzureStorageAccountOptionsValidator()
    {
        RuleFor(x => x.Name)
            .NotNull()
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.ACCOUNT_NAME);

        RuleForEach(x => x.ContainerNames)
            .NotEmpty()
            .Matches(Regexes.Azure.StorageAccount.CONTAINERNAME);
    }
}
