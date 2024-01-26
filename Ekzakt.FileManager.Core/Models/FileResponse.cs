using Ekzakt.FileManager.Core.Contracts;
using System.Net;

namespace Ekzakt.FileManager.Core.Models;

public class FileResponse : IFileManagerResponse
{
    public FileResponse()
    {
    }


    public FileResponse(string message)
    {
        Message = message;
    }


    public FileResponse(string message, HttpStatusCode statusCode) : this(message)
    {
        StatusCode = statusCode;
    }

    public string Message { get; set; } = string.Empty;

    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public bool IsSuccess =>
        StatusCode == HttpStatusCode.Created ||
        StatusCode == HttpStatusCode.Accepted;

}
