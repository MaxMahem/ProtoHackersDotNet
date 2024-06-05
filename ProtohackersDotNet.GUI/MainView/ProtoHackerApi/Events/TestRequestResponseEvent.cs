using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.ApiMessages;

namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;

public class TestRequestResponseEvent(IServer server, Uri api, TestRequestApiResponse response, TimeSpan pollInterval) 
    : TestEvent(server, api)
{
    public override MessageType MessageType => MessageType.Notice;

    public override string Type => nameof(TestRequestResponseEvent);

    public override string Message { get; } 
        = $"Testing begun for {server.Name}:{response.SubmissionId}. Polling every {pollInterval.Seconds}s";
}
