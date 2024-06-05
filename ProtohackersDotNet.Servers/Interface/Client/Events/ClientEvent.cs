namespace ProtoHackersDotNet.Servers.Interface.Client.Events;

public abstract class ClientEvent(IClient client) : IEvent
{
    public IPEndPoint ClientEndPoint { get; } = client.ClientEndPoint;
    public abstract string Source { get; }
    public abstract string? Destination { get; }
    public abstract MessageSource SourceType { get; }
    public abstract MessageType MessageType { get; }
    public abstract ClientEventType ClientEventType { get; }
    public abstract string Type { get; }
    public abstract string Message { get; }

    public bool IsError => false;
    public bool IsSuccess => false;
}