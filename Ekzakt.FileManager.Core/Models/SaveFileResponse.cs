using Ekzakt.FileManager.Core.Contracts;
using System.Net;

namespace Ekzakt.FileManager.Core.Models;

public class SaveFileResponse : IFileManagerResponse
{

    public SaveFileResponse() 
    {
    }


    public SaveFileResponse(string message)
    {
        Message = message;
    }


    public SaveFileResponse(string message, HttpStatusCode statusCode) : this(message)
    {
        StatusCode = statusCode;
    }


    public string FileName { get; set; } = "TestValue";

    public string Message { get; set; } = string.Empty;

    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

    public bool IsSuccess => StatusCode == HttpStatusCode.Created;
}