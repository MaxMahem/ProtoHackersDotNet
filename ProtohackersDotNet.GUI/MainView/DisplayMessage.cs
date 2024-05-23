using ProtoHackersDotNet.Servers.Interfaces;

namespace ProtoHackersDotNet.GUI.MainView;

public class DisplayMessage
{
    public required string Source { get; init; }
    public required string Timestamp { get; init; }
    public required string Type { get; init; }
    public required string Message { get; init; }
}

public static class DisplayMessageHelper
{
    public static DisplayMessage ToDisplayMessage(this IDisplayMessage message) => new() {
        Source = message.Source,
        Type = message.Type,
        Timestamp = message.Timestamp.ToString("HH:mm:ss.ff"),
        Message = message.Message.Trim(),
    };

    public static DisplayMessage ToDisplayMessage(this Exception exception) => new() {
        Source = exception.Source ?? string.Empty,
        Type = exception.GetType().Name,
        Timestamp = DateTime.UtcNow.ToString("HH:mm:ss.ff"),
        Message = exception.Message,
    };
}