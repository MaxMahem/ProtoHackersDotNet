using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.ApiMessages;
namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;

public class TestResultEvent(IServer server, Uri api, ApiCheckResponse response) : TestEvent(server, api)
{
    public override DateTimeOffset Timestamp { get; protected set; } = response.FinishedAt!.Value;

    public override MessageCategory Category { get; } 
        = response.CheckStatus is CheckStatus.Pass ? MessageCategory.Success : MessageCategory.Error;

    public override string Type => nameof(TestResultEvent);

    public override string Message { get; } = $"Testing finished for {server.Name}:{response.SubmissionId}. {response.CheckStatus} {response.Message}";
}
