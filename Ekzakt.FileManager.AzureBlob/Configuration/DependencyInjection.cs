using Ekzakt.FileManager.AzureBlob.Operations;
using Ekzakt.FileManager.AzureBlob.Services;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Models.Models;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models.Responses;
using Ekzakt.FileManager.Core.Options;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Ekzakt.FileManager.AzureBlob.Configuration;

public static class DependencyInjection
{

    [Obsolete("Use AddEkzaktFileManagerAzure instead. This method will be removed in a future version.")]
    public static IServiceCollection AddAzureBlobFileManager(this IServiceCollection services, Action<FileManagerOptions> options)
    {
        services.Configure(options);

        services.AddAzureBlobFileManager();

        return services;
    }

    public static IServiceCollection AddEkzaktFileManagerAzure(this IServiceCollection services, Action<FileManagerOptions> options)
    {
        services.Configure(options);

        services.AddEkzaktFileManagerAzure();

        return services;
    }


    [Obsolete("Use AddEkzaktFileManagerAzure instead. This method will be removed in a furure version.")]
    public static IServiceCollection AddAzureBlobFileManager(this IServiceCollection services, string? configSectionPath = null)
    {
        configSectionPath ??= FileManagerOptions.SectionName;

        services
            .AddOptions<FileManagerOptions>()
            .ValidateOnStart()
            .BindConfiguration(configSectionPath);
        
        // TODO: GitHub issue #9.

        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<IEkzaktFileManager, AzureBlobFileManager>();

        services.AddScoped<IFileOperation<SaveFileRequest, string?>, SaveFileOperation>();
        services.AddScoped<IFileOperation<SaveFileChunkedRequest, string?>, SaveFileChunkedOperation>();
        services.AddScoped<IFileOperation<ListFilesRequest, IEnumerable<FileProperties>?>, ListFilesOperation>();
        services.AddScoped<IFileOperation<DeleteFileRequest, string?>, DeleteFileOperation>();
        services.AddScoped<IFileOperation<DownloadSasTokenRequest, DownloadSasTokenResponse?>, DownloadSasTokenFileOperation>();
        services.AddScoped<IFileOperation<ReadFileAsStringRequest, string?>, ReadFileAsStringOperation>();

        return services;
    }


    public static IServiceCollection AddEkzaktFileManagerAzure(this IServiceCollection services, string? configSectionPath = null)
    {
        configSectionPath ??= FileManagerOptions.SectionName;

        services
            .AddOptions<FileManagerOptions>()
            .ValidateOnStart()
            .BindConfiguration(configSectionPath);

        // TODO: GitHub issue #9.

        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<IEkzaktFileManager, EkzaktFileManagerAzure>();

        services.AddScoped<IFileOperation<SaveFileRequest, string?>, SaveFileOperation>();
        services.AddScoped<IFileOperation<SaveFileChunkedRequest, string?>, SaveFileChunkedOperation>();
        services.AddScoped<IFileOperation<ListFilesRequest, IEnumerable<FileProperties>?>, ListFilesOperation>();
        services.AddScoped<IFileOperation<DeleteFileRequest, string?>, DeleteFileOperation>();
        services.AddScoped<IFileOperation<DownloadSasTokenRequest, DownloadSasTokenResponse?>, DownloadSasTokenFileOperation>();
        services.AddScoped<IFileOperation<ReadFileAsStringRequest, string?>, ReadFileAsStringOperation>();

        return services;
    }
}
