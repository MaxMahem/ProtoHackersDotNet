namespace ProtoHackersDotNet.Servers.Interfaces.Server;

public abstract class ServerEvent(IServer server) : IDisplayMessage
{
    public Problem Problem { get; } = server.Problem;
    public IServer Server { get; } = server;
    public EndPoint? EndPoint { get; } = server.EndPoint;
    public string Source { get; } = server.EndPoint?.ToString() ?? string.Empty;
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public virtual ServerEventType EventType { get; }
    public abstract DisplayMessageType MessageType { get; }
    public string Type => MessageType.ToString();
    public abstract string Message { get; }
}