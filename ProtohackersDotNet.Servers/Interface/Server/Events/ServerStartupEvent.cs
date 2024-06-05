namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ServerStartupEvent(IServer server) : ServerEvent(server)
{
    public override string Type => nameof(ServerStartupEvent);

    public override MessageType MessageType => MessageType.Notice;

    public override string Message { get; }
        = $"{server.Name} started and listening on {server.LocalEndPoint}";
}