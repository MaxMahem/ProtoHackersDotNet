using ProtoHackersDotNet.GUI.MainView.ApiTest.ApiRequests;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest.Messages;

public class TestResultMessage(IServer server, Uri api, ApiCheckResponse response) : TestEvent(server, api)
{
    public override DisplayMessageType MessageType => DisplayMessageType.TestResult;

    public override DateTime Timestamp { get; protected set; } = response.FinishedAt!.Value;

    public override string Message { get; } = $"Testing finished for {server.Name}:{response.SubmissionId}. {response.CheckStatus} {response.Message}".Trim();
}
