namespace ProtoHackersDotNet.Servers.Interfaces;

public interface IDisplayMessage
{
    int ProblemId { get; }
    string Source { get; }
    DisplayMessageType MessageType { get; }
    string Type => MessageType.ToString();
    DateTime Timestamp { get; }
    string Message { get; }
}
