using Ekzakt.FileManager.Core.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Ekzakt.FileManager.Core.Extensions;

public static class HttpRequestExtensions
{
    public static async Task<List<SaveFileRequest>> TryParseSaveFileRequests(this HttpRequest httpRequest, CancellationToken cancellationToken = default)
    {
        var output = new List<SaveFileRequest>();
        var contentType = httpRequest.ContentType;

        if (contentType is null)
        {
            throw new InvalidDataException($"{nameof(httpRequest.ContentType)} is null.");
        }

        var boundary = GetMultipartBoundary(MediaTypeHeaderValue.Parse(contentType));
        var multipartReader = new MultipartReader(boundary, httpRequest.Body);

        var section = await multipartReader.ReadNextSectionAsync();

        while (section != null)
        {
            var contentDisposition = section.GetContentDispositionHeader();

            if (contentDisposition!.IsFormDisposition())
            {
                var jsonString = section.ReadAsStringAsync(cancellationToken);
            }
            else if (contentDisposition!.IsFileDisposition())
            {
                var fileName = contentDisposition!.FileName.Value;
            }

            section = await multipartReader.ReadNextSectionAsync();
        }

        return output;
    }



    #region Helpers

    internal static string GetMultipartBoundary(MediaTypeHeaderValue contentType)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        return boundary;
    }

    #endregion Helpers
}
