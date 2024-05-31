namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;

public abstract class TestEvent(IServer server, Uri api) : IDisplayEvent
{
    public ServerName ServerName { get; } = server.Name;
    public Problem Problem { get; } = server.Solution;
    public Uri Api { get; } = api;
    public virtual string Source => Api.Host;
    public virtual string Destination { get; } = server.Name.ToString();
    public virtual DateTimeOffset Timestamp { get; protected set; } = DateTimeOffset.UtcNow;
    public abstract MessageCategory Category { get; }
    public MessageSource MessageSource => MessageSource.TestApi;
    public abstract string Type { get; }
    public abstract string Message { get; }
}