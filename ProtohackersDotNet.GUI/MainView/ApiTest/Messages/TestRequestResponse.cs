using ProtoHackersDotNet.GUI.MainView.ApiTest.ApiRequests;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest.Messages;

public class TestRequestResponse(IServer server, Uri api, TestRequestApiResponse response) : TestEvent(server, api)
{
    public override DisplayMessageType MessageType => DisplayMessageType.TestRequestResponse;

    public override string Message { get; } = $"Testing begun for {server.Name}:{response.SubmissionId}.";
}
