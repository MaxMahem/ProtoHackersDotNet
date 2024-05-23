using ProtoHackersDotNet.GUI.MainView.ApiTest.ApiMessages;
using ProtoHackersDotNet.Servers.Interfaces;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest.Messages;

public abstract class TestEvent(IServer server, Uri api) : IDisplayMessage
{
    public int ProblemId { get; } = server.ProblemId;
    public Uri Api { get; } = api;
    public virtual string Source => Api.ToString();
    public virtual DateTime Timestamp { get; protected set; } = DateTime.UtcNow;
    public abstract DisplayMessageType MessageType { get; }
    public abstract string Message { get; }
}

public class TestRequested(IServer server, Uri api, TestApiRequest request) : TestEvent(server, api)
{
    public override string Source { get; } = server.EndPoint?.ToString() ?? ThrowHelper.ThrowArgumentNullException<string>();
    public override DisplayMessageType MessageType => DisplayMessageType.TestRequest;

    public override string Message { get; } = $"Test requested for {server.Name} on {request.Ip}:{request.Port}.";
}