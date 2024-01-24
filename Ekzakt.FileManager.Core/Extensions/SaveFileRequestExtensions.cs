using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Validators;

namespace Ekzakt.FileManager.Core.Extensions;

public static class SaveFileRequestExtensions
{
    public static bool TryValidate(this SaveFileRequest request, out SaveFileResponse? response)
    {
        var validator = new SaveFileRequestValidator();
        var result = validator.Validate(request);

        if (!result.IsValid)
        {
            var message = result.Errors.FirstOrDefault();

            response = new SaveFileResponse()
            {
                FileName = request.FileName,
                Message = message?.ErrorMessage ?? string.Empty,
                HttpStatusCode = System.Net.HttpStatusCode.BadRequest
            };
            return false;
        }

        response = null;
        
        return true;
    }
}
