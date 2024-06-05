namespace ProtoHackersDotNet.Servers.Interface.Server.Events;

public class ServerTerminatedEvent(IServer server, Exception exception) : ServerEvent(server)
{
    public override string Type => nameof(ServerTerminatedEvent);

    public override MessageType MessageType => MessageType.Error;

    public override string Message { get; } 
        = $"{server.Name} terminated due to unhandled exception '{exception.GetType().Name}'";
}