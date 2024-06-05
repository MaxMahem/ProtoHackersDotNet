namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public abstract class ServerEvent(IServer server) : IEvent
{
    public ServerName ServerName { get; } = server.Name;
    public Problem Problem { get; } = server.Solution;
    public string Source { get; } = server.Name.ToString();

    public abstract string Type { get; }
    public MessageSource SourceType => MessageSource.Server;
    public abstract MessageType MessageType { get; }
    public abstract string Message { get; }
    public bool IsError => false;
    public bool IsSuccess => false;
}