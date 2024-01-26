using System.Net;

namespace Ekzakt.FileManager.Core.Contracts;

public interface IFileManagerResponse
{
    string Message { get; set; }

    HttpStatusCode StatusCode { get; set; }

    bool IsSuccess { get; }
}
