using Material.Icons;
using ProtoHackersDotNet.GUI.MainView.Grader.Events;

namespace ProtoHackersDotNet.GUI.MainView.Messages;

public record class MessageVM : IComparable<MessageVM>
{
    private static int currentId;
    public static int NextId => Interlocked.Increment(ref currentId);

    public int Id { get; } = NextId;
    public required MessageSource MessageSource { get; init; }
    public required string Source { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string Type { get; init; }
    public required MaterialIconKind Icon { get; init; }
    public required string Message { get; init; }
    public bool IsError { get; init; }
    public bool IsSuccess { get; init; }
    public bool IsServer { get; init; } = false;
    public bool IsClient { get; init; } = false;
    public bool IsTest { get; init; } = false;

    public int CompareTo(MessageVM? other) => other is null ? +1 : Timestamp.CompareTo(other.Timestamp);

    public static MessageVM FromEvent(Timestamped<IEvent> displayEvent) => new() {
        MessageSource = displayEvent.Value.SourceType,
        Source = displayEvent.Value.Source,
        Timestamp = displayEvent.Value switch {
            GradingLogMessage logMessage => logMessage.Timestamp,
            _ => displayEvent.Timestamp,
        },
        Type = displayEvent.Value.Type,
        Message = displayEvent.Value.Message.Trim(),
        Icon = displayEvent.Value switch {
            _ when displayEvent.Value.MessageType is MessageType.Error => MaterialIconKind.Error,
            ServerStartupEvent or ServerShutdownEvent => MaterialIconKind.Power,
            DataTransmissionEvent => MaterialIconKind.Download,
            DataReceptionEvent => MaterialIconKind.Upload,
            DataBroadcastEvent => MaterialIconKind.Broadcast,
            ClientConnectionEvent => MaterialIconKind.Link,
            ClientDisconnectEvent => MaterialIconKind.LinkOff,
            GradingRequestEvent => MaterialIconKind.ReceiptTextArrowRightOutline,
            GradingRequestResponseEvent => MaterialIconKind.ReceiptTextArrowLeftOutline,
            GradingLogMessage { MessageType: var type } => type switch {
                MessageType.Success => MaterialIconKind.ReceiptTextCheckOutline,
                MessageType.Error => MaterialIconKind.ReceiptTextRemoveOutline,
                MessageType.Notice => MaterialIconKind.ReceiptTextOutline,
                _ => ThrowArgumentException<MaterialIconKind>($"Invalid message type {type} for log message."),
            },
            GradingResultEvent { MessageType: var type } => type switch {
                MessageType.Success => MaterialIconKind.Success,
                MessageType.Error => MaterialIconKind.AlertCircleOutline,
                _ => ThrowArgumentException<MaterialIconKind>($"Invalid message type {type} for result message."),
            },
            _ => MaterialIconKind.Error,
        },
        IsError = displayEvent.Value.MessageType is MessageType.Error,
        IsSuccess = displayEvent.Value.MessageType is MessageType.Success,
        IsServer = displayEvent.Value.SourceType is MessageSource.Server,
        IsClient = displayEvent.Value.SourceType is MessageSource.Client,
        IsTest = displayEvent.Value.SourceType is MessageSource.Grader,
    };

    public static MessageVM FromException(Exception exception, MessageSource sourceType, string sourceName) => new() {
        MessageSource = sourceType,
        Source = sourceName,
        Timestamp = DateTime.UtcNow,
        Type = exception.GetType().Name,
        Message = exception.Message,
        Icon = MaterialIconKind.Error,
        IsError = true,
        IsSuccess = false,
    };
}