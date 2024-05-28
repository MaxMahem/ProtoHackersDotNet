namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;

public class ProtoHackerApiException(IServer server, Uri? api, Exception exception) : Exception(exception.Message, exception)
{
    public IServer Server { get; } = server;
    public Uri? Api { get; } = api;

    public static ProtoHackerApiException FromException(IServer server, Uri? api, Exception exception)
        => new(server, api, exception);
}