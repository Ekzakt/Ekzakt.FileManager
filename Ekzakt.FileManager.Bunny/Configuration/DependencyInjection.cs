using Ekzakt.FileManager.Bunny.Operations;
using Ekzakt.FileManager.Core.Contracts;
using Ekzakt.FileManager.Core.Models.Requests;
using Ekzakt.FileManager.Core.Models;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Ekzakt.FileManager.Bunny.Services;
using Microsoft.Extensions.Configuration;
using Polly;
using Ekzakt.FileManager.Bunny.HttpClients;

namespace Ekzakt.FileManager.Bunny.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddEkzaktFileManagerBunny(this IServiceCollection services, IConfiguration configuration, string? configSectionPath = null)
    {
        configSectionPath ??= EkzaktFileManagerBunnyOptions.SectionName;

        services
            .AddOptions<EkzaktFileManagerBunnyOptions>()
            .BindConfiguration(configSectionPath);

        var options = configuration
            .GetSection(configSectionPath)
            .Get<EkzaktFileManagerBunnyOptions>();

        services.AddBunnyHttpClient(options);

        // TODO: GitHub issue #9.

        services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

        services.AddScoped<IEkzaktFileManager, EkzaktFileManagerBunny>();
        services.AddScoped<IFileOperation<SaveFileRequest, string?>, SaveFileOperation>();
        //services.AddScoped<IFileOperation<SaveFileChunkedRequest, string?>, SaveFileChunkedOperation>();
        services.AddScoped<IFileOperation<ListFilesRequest, IEnumerable<FileInformation>?>, ListFilesOperation>();
        services.AddScoped<IFileOperation<DeleteFileRequest, string?>, DeleteFileOperation>();
        //services.AddScoped<IFileOperation<DownloadSasTokenRequest, DownloadSasTokenResponse?>, DownloadSasTokenFileOperation>();
        //services.AddScoped<IFileOperation<ReadFileAsStringRequest, string?>, ReadFileAsStringOperation>();

        return services;
    }


    #region Helpers

    private static IServiceCollection AddBunnyHttpClient(this IServiceCollection services, EkzaktFileManagerBunnyOptions? options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        var retryTimeSpans = GetRetryTimeSpans(options);

        services
            .AddHttpClient<BunnyHttpClient>(config =>
            {
                //config.BaseAddress = new Uri(options.BaseAddress);
                config.DefaultRequestHeaders.Add("accept", "application/json");
                config.DefaultRequestHeaders.Add("AccessKey", options.ApiKey);
                config.Timeout = TimeSpan.FromSeconds(options.HttpTimeout);
            })
            .AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.WaitAndRetryAsync(retryTimeSpans));

        return services;
    }

    private static List<TimeSpan> GetRetryTimeSpans(EkzaktFileManagerBunnyOptions? options)
    {
        List<TimeSpan> timeSpans = [];

        if (options is not null && options.RetryPolicy.Length > 0)
        {
            foreach (var item in options.RetryPolicy)
            {
                timeSpans.Add(TimeSpan.FromSeconds(item));
            }
        }

        return timeSpans;
    }

    #endregion Helpers
}
