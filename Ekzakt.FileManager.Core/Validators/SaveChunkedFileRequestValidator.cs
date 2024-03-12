using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;
using Regexes = Ekzakt.Utilities.Validation.Regex;

namespace Ekzakt.FileManager.Core.Validators;

public class SaveChunkedFileRequestValidator : AbstractValidator<SaveFileChunkedRequest>
{
    public SaveChunkedFileRequestValidator()
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
            .GreaterThan(0);

        RuleFor(x => x.ChunkIndex)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ChunkTreshold)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ChunkSize)
            .GreaterThanOrEqualTo(0);

    }
}
