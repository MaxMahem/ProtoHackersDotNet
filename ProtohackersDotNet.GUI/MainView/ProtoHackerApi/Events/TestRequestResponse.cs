using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.ApiMessages;

namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;

public class TestRequestResponse(IServer server, Uri api, TestRequestApiResponse response) : TestEvent(server, api)
{
    public override MessageCategory Category => MessageCategory.Notice;

    public override string Type => nameof(TestRequestResponse);

    public override string Message { get; } = $"Testing begun for {server.Name}:{response.SubmissionId}";
}
