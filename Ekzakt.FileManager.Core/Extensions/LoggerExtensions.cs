using Ekzakt.FileManager.Core.Models.Requests;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ekzakt.FileManager.Core.Extensions;

public static class LoggerExtensions
{
    public static void LogRequestStarted<TLogger, TRequest>(this ILogger<TLogger> logger, TRequest? request)
        where TLogger : class
        where TRequest : AbstractFileRequest?
    {
        logger.LogDebug("{RequestName} started. Request: {saveFileRequest}",
                typeof(TRequest).Name,
                JsonSerializer.Serialize(request));
    }


    public static void LogRequestFinished<TLogger, TRequest>(this ILogger<TLogger> logger, TRequest? request)
        where TLogger : class
        where TRequest : AbstractFileRequest?
    {
        logger.LogDebug("{RequestName} finished.",
                typeof(TRequest).Name);
    }
}
