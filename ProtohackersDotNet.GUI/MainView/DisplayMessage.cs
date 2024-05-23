using ProtoHackersDotNet.Servers.Interfaces;

namespace ProtoHackersDotNet.GUI.MainView;

public class DisplayMessage
{
    public required int ProblemId { get; init; }
    public required string Source { get; init; }
    public required string Timestamp { get; init; }
    public required string Type { get; init; }
    public required string Message { get; init; }
}

public static class DisplayMessageHelper
{
    public static DisplayMessage ToDisplayMessage(this IDisplayMessage message) => new() {
        ProblemId = message.ProblemId,
        Source = message.Source,
        Type = message.Type,
        Timestamp = message.Timestamp.ToString("HH:mm:ss.ff"),
        Message = message.Message.Trim(),
    };
}