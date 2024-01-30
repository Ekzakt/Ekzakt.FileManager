﻿using Ekzakt.FileManager.Core.Contracts;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Ekzakt.FileManager.Core.Extensions;

public static class LoggerExtensions
{
    public static void LogRequestStarted<TLogger, TRequest>(this ILogger<TLogger> logger, TRequest? request)
        where TLogger : class
        where TRequest : AbstractFileRequest?
    {
        logger.LogInformation("{RequestName} started. CorrelationId: {CorrelationId}, Request: {saveFileRequest}",
                typeof(TRequest).Name,
                request!.CorrelationId,
                JsonSerializer.Serialize(request));
    }


    public static void LogRequestFinished<TLogger, TRequest>(this ILogger<TLogger> logger, TRequest? request)
        where TLogger : class
        where TRequest : AbstractFileRequest?

    {
        logger.LogInformation("{RequestName} finished. CorrelationId: {CorrelationId}",
                typeof(TRequest).Name,
                request!.CorrelationId);
    }


}