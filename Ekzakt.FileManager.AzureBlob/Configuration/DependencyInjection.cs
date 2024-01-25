using Ekzakt.FileManager.Core.Contracts;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Ekzakt.FileManager.AzureBlob.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddAzureBlobFileManager(this IServiceCollection services, Action<IFileManagerOptions> options)
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

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IFileManager, AzureBlobFileManager>();

        return services;
    }
}
