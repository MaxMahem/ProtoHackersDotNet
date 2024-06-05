namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;

public abstract class TestEvent(IServer server, Uri api) : IEvent
{
    public ServerName ServerName { get; } = server.Name;
    public Problem Problem { get; } = server.Solution;
    public virtual string Source { get; } = api.Host;
    public virtual string Destination { get; } = server.Name.ToString();
    public virtual DateTimeOffset Timestamp { get; protected set; } = DateTimeOffset.UtcNow;
    public abstract MessageType MessageType { get; }
    public MessageSource SourceType => MessageSource.TestApi;
    public abstract string Type { get; }
    public abstract string Message { get; }
}