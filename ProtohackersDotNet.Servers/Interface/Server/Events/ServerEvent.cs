namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public abstract class ServerEvent(IServer server) : IDisplayEvent
{
    public ServerName ServerName { get; } = server.Name;
    public Problem Problem { get; } = server.Solution;
    public IServer Server { get; } = server;
    public EndPoint? EndPoint { get; } = server.LocalEndPoint;
    public string Source { get; } = server.Name.ToString();

    public abstract ServerEventType EventType { get; }
    public abstract string Type { get; }
    public MessageSource MessageSource => MessageSource.Server;
    public abstract MessageCategory Category { get; }
    public abstract string Message { get; }
}