using Ekzakt.FileManager.Bunny.Configuration;
using Ekzakt.FileManager.Core.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ekzakt.FileManager.Bunny.Operations;

public abstract class AbstractBunnyFileOperation<TLogger> : AbstractFileOperation<TLogger>
    where TLogger : class
{
    private readonly ILogger _logger;
    private readonly EkzaktFileManagerBunnyOptions _options;

    private Uri? _bunnyUri = null;

    public Uri? BunnyUri => _bunnyUri;
    
    protected AbstractBunnyFileOperation(
        ILogger<TLogger> logger,
        IOptions<EkzaktFileManagerBunnyOptions> options) : base(logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value ?? throw new ArgumentNullException(nameof(_options));
    }


    public Uri GetBunnyUri(params string[] paths)
    {
        var baseUri = GetBunnyBaseUri(_options.MainReplicationRegion);
        var uri = new Uri(baseUri, Path.Combine(paths));

        return uri;
    }


    #region Helpers

    private Uri GetBunnyBaseUri(string mainReplicationRegion)
    {
        var uriBuilder = new UriBuilder();

        uriBuilder.Scheme = Constants.BUNNY_HTTP_PROTOCOL;

        if (mainReplicationRegion == string.Empty || 
            mainReplicationRegion.ToLower() == Constants.BUNNY_MAIN_REPLICATION_REGION.ToLower())
        {
            uriBuilder.Host = GetBunnyBasePath();
            return uriBuilder.Uri;
        }

        uriBuilder.Host = $"{mainReplicationRegion}.{GetBunnyBasePath()}";
        return uriBuilder.Uri;
    }

    private string GetBunnyBasePath()
    {
        var path = $"{Constants.BUNNY_BASE_URI}/{_options.BaseStorageZoneName}/{_options.BaseLocation}";
        return path;
    }

    #endregion Helpers
}
