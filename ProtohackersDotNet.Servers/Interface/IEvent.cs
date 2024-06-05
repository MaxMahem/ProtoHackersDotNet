namespace ProtoHackersDotNet.Servers.Interface;

public interface IEvent
{
    string Source { get; }
    MessageSource SourceType { get; }
    MessageType MessageType { get; }
    string Type { get; }
    string Message { get; }
}