namespace ProtoHackersDotNet.GUI.MainView.Grader.Events;

public abstract class GradingEvent(Uri api) : IEvent
{
    public virtual string Source { get; } = api.Host;
    public virtual DateTimeOffset Timestamp { get; protected set; } = DateTimeOffset.UtcNow;
    public abstract MessageType MessageType { get; }
    public MessageSource SourceType => MessageSource.Grader;
    public abstract string Type { get; }
    public abstract string Message { get; }
}