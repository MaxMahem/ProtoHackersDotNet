using ProtoHackersDotNet.Servers.Interfaces;

namespace ProtoHackersDotNet.GUI.MainView.Client.Messages;

public abstract class ClientEvent(IClient client) : IDisplayMessage
{
    public int ProblemId { get; } = client.ProblemId;
    public Guid Guid { get; } = client.Id;
    public EndPoint? EndPoint { get; } = client.EndPoint;
    public string Source => EndPoint?.ToString() ?? string.Empty;
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public abstract DisplayMessageType MessageType { get; }
    public abstract string Message { get; }
}
