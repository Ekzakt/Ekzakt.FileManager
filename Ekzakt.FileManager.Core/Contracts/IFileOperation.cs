using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;

namespace Ekzakt.FileManager.Core.Contracts;

public interface IFileOperation<TRequest, TResponse>
    where TRequest : AbstractFileRequest
    where TResponse : class?
{
    Task<FileResponse<TResponse>> ExecuteAsync(TRequest request, CancellationToken cancellationToken = default);
}