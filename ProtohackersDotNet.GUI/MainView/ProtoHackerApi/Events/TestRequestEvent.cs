namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;

public class TestRequestEvent(IServer server, Uri api) : TestEvent(server, api)
{
    public override string Type => nameof(TestRequestEvent);
    public override string Source { get; } = server.Name.ToString();
    public override MessageType MessageType => MessageType.Notice;
    public override string Message { get; } = $"Test requested for {server.Name} on {server.LocalEndPoint}";
}