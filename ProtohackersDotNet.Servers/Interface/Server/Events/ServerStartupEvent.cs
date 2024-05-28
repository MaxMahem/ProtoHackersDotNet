namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ServerStartupEvent(IServer server) : ServerEvent(server)
{
    public override ServerEventType EventType { get; } = ServerEventType.Start;
    public override string Source { get; } = server.Name.ToString();
    public override string Type { get; } = $"Server{ServerEventType.Start}";

    public override MessageCategory Category => MessageCategory.StatusChange;

    public override string Message { get; } = $"{server.Name} started and listening on {server.LocalEndPoint}";
}
