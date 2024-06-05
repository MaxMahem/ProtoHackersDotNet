namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ServerShutdownEvent(IServer server) : ServerEvent(server)
{
    public override string Type => nameof(ServerShutdownEvent);

    public override MessageType MessageType => MessageType.Notice;

    public override string Message { get; } = $"{server.Name} stopped";
}
