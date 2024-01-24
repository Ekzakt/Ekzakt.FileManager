using Ekzakt.FileManager.AzureBlob.Configuration;
using FluentValidation;
using System.Data;

namespace Ekzakt.FileManager.AzureBlob.Validators;

public class AzureFileManagerOptionsValidator : AbstractValidator<AzureFileManagerOptions>
{
    public AzureFileManagerOptionsValidator()
    {
        RuleFor(x => x.Azure)
            .NotEmpty()
            .SetValidator(new AzureOptionsValidator());
    }
}
