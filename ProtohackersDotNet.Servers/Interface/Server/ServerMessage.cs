namespace ProtoHackersDotNet.Servers.Interface.Server;

public class ServerMessage : IDisplayMessage
{
    public required EndPoint? EndPoint { get; init; }
    public DateTime TimeStamp => DateTime.UtcNow;
    public required ServerEventType EventType { get; init; }
    public string Type => EventType.ToString();
    public required string Message { get; init; }
}