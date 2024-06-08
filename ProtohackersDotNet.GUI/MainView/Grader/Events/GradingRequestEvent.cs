namespace ProtoHackersDotNet.GUI.MainView.Grader.Events;

public class GradingRequestEvent(IServer server, Uri api) : GradingEvent(api)
{
    public override string Type => nameof(GradingRequestEvent);
    public override string Source { get; } = server.Name.ToString();
    public override MessageType MessageType => MessageType.Notice;
    public override string Message { get; } = $"Test requested for {server.Name} on {server.LocalEndPoint}";
}