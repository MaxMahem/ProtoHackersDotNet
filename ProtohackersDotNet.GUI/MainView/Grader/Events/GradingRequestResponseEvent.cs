using ProtoHackersDotNet.GUI.MainView.Grader.Messages;

namespace ProtoHackersDotNet.GUI.MainView.Grader.Events;

public class GradingRequestResponseEvent(IServer server, Uri api, GradingRequestResponse response, TimeSpan pollInterval) 
    : GradingEvent(api)
{
    public override MessageType MessageType => MessageType.Notice;

    public override string Type => nameof(GradingRequestResponseEvent);

    public override string Message { get; } 
        = $"Testing begun for {server.Name}:{response.SubmissionId}. Polling every {pollInterval.Seconds}s";
}
