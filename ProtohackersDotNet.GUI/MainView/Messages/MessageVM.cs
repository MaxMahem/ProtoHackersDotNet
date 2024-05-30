using Material.Icons;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;
using ProtoHackersDotNet.Servers.Interface.Client.Events;
using ProtoHackersDotNet.Servers.Interface.Server.Events;

namespace ProtoHackersDotNet.GUI.MainView.Messages;

public record class MessageVM
{
    private static int currentId;
    public static int NextId => Interlocked.Increment(ref currentId);

    public int Id { get; } = NextId;
    public required string Source { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    public required string Type { get; init; }
    public required MaterialIconKind Icon { get; init; }
    public required string Message { get; init; }
    public bool IsError { get; init; } = false;
    public bool IsSuccess { get; init; } = false;

    public static MessageVM FromClientEvent(ClientEvent clientEvent) => new() {
        Source = clientEvent.Source,
        Timestamp = clientEvent.Timestamp,
        Type = clientEvent.Type,
        Icon = clientEvent.ClientEventType switch {
            ClientEventType.DataTransmitted => MaterialIconKind.Download,
            ClientEventType.DataReceived => MaterialIconKind.Upload,
            _ => MaterialIconKind.Error,
        },
        Message = clientEvent.Message.Trim(),
    };

    public static MessageVM FromSeverEvent(ServerEvent serverEvent) => new() {
        Source = serverEvent.Source,
        Timestamp = serverEvent.Timestamp,
        Type = serverEvent.Type,
        Icon = serverEvent.EventType switch {
            ServerEventType.ClientConnect => MaterialIconKind.Link,
            ServerEventType.ClientDisconnect => MaterialIconKind.LinkOff,
            ServerEventType.Broadcast => MaterialIconKind.Broadcast,
            ServerEventType.Start => MaterialIconKind.Power,
            ServerEventType.Stop => MaterialIconKind.Power,
            _ => MaterialIconKind.Error,
        },
        Message = serverEvent.Message.Trim(),
    };

    public static MessageVM FromTestEvent(TestEvent testEvent) => new() {
        Source = testEvent.Source,
        Timestamp = testEvent.Timestamp,
        Type = testEvent.Type,
        Icon = testEvent switch {
            TestRequestEvent => MaterialIconKind.ReceiptTextOutline,
            TestRequestResponse => MaterialIconKind.ReceiptTextOutline,
            TestResultEvent result => result.Category is MessageCategory.Success ? MaterialIconKind.Success : MaterialIconKind.Error,
            TestLogMessage result => MaterialIconKind.ReceiptTextOutline,
            _ => MaterialIconKind.Error,
        },
        Message = testEvent.Message.Trim(),
        IsError = testEvent.Category is MessageCategory.Error,
        IsSuccess = testEvent.Category is MessageCategory.Success,
    };

    public static MessageVM FromException(Exception exception, string source) => new() {
        Source = source,
        Type = exception.InnerException?.GetType().Name ?? exception.GetType().Name,
        Message = exception.Message.Trim(),
        Icon = MaterialIconKind.Error,
        IsError = true,
    };
}