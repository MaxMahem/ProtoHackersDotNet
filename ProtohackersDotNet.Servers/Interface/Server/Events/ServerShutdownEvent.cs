namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ServerShutdownEvent(IServer server) : ServerEvent(server)
{
    public override ServerEventType EventType { get; } = ServerEventType.Stop;
    public override string Source { get; } = server.Name.ToString();
    public override string Type { get; } = $"Server{ServerEventType.Stop}";
    public override MessageCategory Category => MessageCategory.StatusChange;
    public override string Message { get; } = $"{server.Name} stopped";
}