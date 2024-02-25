using System.Net;

namespace Ekzakt.FileManager.Core.Models.Responses;

public class FileResponse<T>
    where T : class?
{
    public HttpStatusCode Status { get; set; }

    public T? Data { get; set; }

    public string Message { get; set; } = string.Empty;

    public bool IsSuccess() =>  Status >= HttpStatusCode.OK && Status<HttpStatusCode.Ambiguous;
}
