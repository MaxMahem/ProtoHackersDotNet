namespace ProtoHackersDotNet.Servers.Interface;

public interface IDisplayEvent
{
    Problem Problem { get; }
    ServerName ServerName { get; }
    string Source { get; }
    string? Destination { get; }
    MessageSource MessageSource { get; }
    MessageCategory Category { get; }
    string Type { get; }
    DateTimeOffset Timestamp { get; }
    string Message { get; }
}

public enum MessageSource
{
    Unknown = 0,
    Server,
    LocalClient,
    RemoteClient,
    TestApi
}

public enum MessageCategory
{
    /// <summary>Message indicates some sort of change in status</summary>
    StatusChange,
    /// <summary>Message indicates transmission of data.</summary>
    Data,
    /// <summary>Message is an update to some ongoing event.</summary>
    Notice,
    /// <summary>Message indicates an error.</summary>
    Error,
    /// <summary>Message indicates success of some process.</summary>
    Success,
}