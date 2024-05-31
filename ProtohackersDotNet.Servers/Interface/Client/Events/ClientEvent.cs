namespace ProtoHackersDotNet.Servers.Interface.Client.Events;

public abstract class ClientEvent(IClient client) : IDisplayEvent
{
    public ServerName ServerName { get; } = client.Server.Name;
    public Problem Problem { get; } = client.Server.Solution;
    public Guid ClientId { get; } = client.Id;
    public IPEndPoint ClientEndPoint { get; } = client.ClientEndPoint;
    public abstract string Source { get; }
    public abstract string? Destination { get; }
    public abstract MessageSource MessageSource { get; }
    public abstract MessageCategory Category { get; }
    public abstract ClientEventType ClientEventType { get; }
    public abstract string Type { get; }
    public abstract string Message { get; }
}