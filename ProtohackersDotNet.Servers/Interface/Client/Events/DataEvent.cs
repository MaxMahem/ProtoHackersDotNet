namespace ProtoHackersDotNet.Servers.Interface.Client.Events;

public abstract class DataEvent(string message) : IEvent
{
    public DataEvent(ITransmission transmission) : this(transmission.Translation) { }
    public required string Source { get; init; }
    public abstract MessageSource SourceType { get; }
    public abstract string Type { get; }
    public MessageType MessageType => MessageType.Notice;
    public string Message { get; } = message;
    public bool IsError => false;
    public bool IsSuccess => false;
}