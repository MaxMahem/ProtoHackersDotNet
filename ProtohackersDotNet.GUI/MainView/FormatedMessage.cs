using ProtoHackersDotNet.Servers.Interface;

namespace ProtoHackersDotNet.GUI.MainView;

public class FormatedMessage()
{
    public required string Source { get; init; }
    public required string TimeStamp { get; init; }
    public required string Type { get; init; }
    public required string Message { get; init; }
}

public static class FormatedMessageHelper
{
    public static FormatedMessage ToFormated(this IDisplayMessage message)
        => new() {
            Source = message.EndPoint?.ToString() ?? string.Empty,
            TimeStamp = message.TimeStamp.ToString("HH:mm:ss.ffff"),
            Type = message.Type,
            Message = message.Message
        };
}