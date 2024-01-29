using Ekzakt.FileManager.AzureBlob.Services;
using Ekzakt.FileManager.Core.Contracts;
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

        return services;
    }
}
