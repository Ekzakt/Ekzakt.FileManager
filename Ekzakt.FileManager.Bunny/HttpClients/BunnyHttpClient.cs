namespace Ekzakt.FileManager.Bunny.HttpClients;

internal class BunnyHttpClient
{
    public HttpClient Client { get; }

    public BunnyHttpClient(HttpClient client)
    {
        Client = client;
    }
}
