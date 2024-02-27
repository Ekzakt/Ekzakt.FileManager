using Ekzakt.FileManager.AzureBlob.Operations;
using Ekzakt.FileManager.AzureBlob.Services;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Options;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Ekzakt.FileManager.AzureBlob.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureBlobFileManager(this IServiceCollection services, Action<FileManagerOptions> options)
    {
        services.Configure(options);

        services.AddAzureBlobFileManager();

        return services;
    }


    public static IServiceCollection AddAzureBlobFileManager(this IServiceCollection services, string? configSectionPath = null)
    {
        configSectionPath ??= FileManagerOptions.SectionName;

        services
            .AddOptions<FileManagerOptions>()
            .ValidateOnStart()
            .BindConfiguration(configSectionPath);

        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<IFileManager, AzureBlobFileManager>();
        services.AddScoped<IFileOperation<SaveFileRequest, string?>, SaveFileOperation>();
        services.AddScoped<IFileOperation<SaveFileChunkedRequest, string?>, SaveFileChunkedOperation>();
        services.AddScoped<IFileOperation<ListFilesRequest, IEnumerable<FileInformation>?>, ListFilesOperation>();
        services.AddScoped<IFileOperation<DeleteFileRequest, string?>, DeleteFileOperation>();
        services.AddScoped<IFileOperation<DownloadFileRequest, DownloadFileResponse?>, DownloadFileOperation>();

        return services;
    }
}
