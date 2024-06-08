namespace ProtoHackersDotNet.GUI.MainView.Grader;

public class GradingException(IServer server, Uri? api, Exception exception) : Exception(exception.Message, exception)
{
    public IServer Server { get; } = server;
    public Uri? Api { get; } = api;

    public static GradingException FromException(IServer server, Uri? api, Exception exception)
        => new(server, api, exception);
}