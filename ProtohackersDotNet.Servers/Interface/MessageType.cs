namespace ProtoHackersDotNet.Servers.Interface;

public enum MessageType
{
    /// <summary>Message is an update to some ongoing event.</summary>
    Notice,
    /// <summary>Message indicates an error.</summary>
    Error,
    /// <summary>Message indicates success of some process.</summary>
    Success,
}