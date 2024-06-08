using ProtoHackersDotNet.GUI.MainView.Grader.Messages;
namespace ProtoHackersDotNet.GUI.MainView.Grader.Events;

public class GradingResultEvent(IServer server, Uri api, GraderResponse response) : GradingEvent(api)
{
    public override DateTimeOffset Timestamp { get; protected set; } = response.FinishedAt!.Value;

    public override MessageType MessageType { get; } = response.CheckStatus switch {
        GraderStatus.Pass => MessageType.Success,
        GraderStatus.Fail => MessageType.Error,
        _ => ThrowArgumentException<MessageType>($"CheckStatus '{response.CheckStatus}' is invalid for {nameof(GradingResultEvent)}"),
    };
    public override string Type => nameof(GradingResultEvent);
    public override string Message { get; } 
        = $"Testing finished for {server.Name}:{response.SubmissionId}. {response.CheckStatus} {response.Message}";
}
