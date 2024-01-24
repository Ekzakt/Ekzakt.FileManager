using Ekzakt.FileManager.AzureBlob.Configuration;
using FluentValidation;

namespace Ekzakt.FileManager.AzureBlob.Validators;

public class AzureOptionsValidator : AbstractValidator<AzureOptions>
{
    public AzureOptionsValidator()
    {
        RuleFor(x => x.StorageAccount)
            .SetValidator(new AzureStorageAccountOptionsValidator());
    }
}
