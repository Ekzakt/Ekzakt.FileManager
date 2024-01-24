using Ekzakt.FileManager.Core.Contracts;
using System.Net;

namespace Ekzakt.FileManager.Core.Models;

public class SaveFileResponse : IFileResponse
{
    public string FileName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public HttpStatusCode HttpStatusCode { get; set; } = HttpStatusCode.InternalServerError;

    public bool IsSuccess => HttpStatusCode == HttpStatusCode.Created;
}
