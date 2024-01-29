using Ekzakt.FileManager.Core.Models.Requests;
using FluentValidation;

namespace Ekzakt.FileManager.Core.Validators
{
    public class ListFilesRequestValidator : AbstractValidator<ListFilesRequest>
    {
        public ListFilesRequestValidator()
        {
            // TODO: Make this simpler!
            RuleFor(request => request.ContainerName).NotEmpty().WithMessage("Container name cannot be empty.");
        }
    }
}