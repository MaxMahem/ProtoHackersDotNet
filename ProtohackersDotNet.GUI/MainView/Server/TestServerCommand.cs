using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;
using ProtoHackersDotNet.GUI.MainView.Messages;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class TestServerCommand(ProtoHackerApiManager protoHackerApiManager, MessageManager messageManager)
{
    public void Test(IServer server, IPEndPoint remoteEndPoint)
    {
        var testEvents = protoHackerApiManager.TestServer(server, remoteEndPoint);
        messageManager.SubscribeToStream(testEvents, MessageVM.FromTestEvent, [.. protoHackerApiManager.EventSources]);
    }

    public IObservable<bool> TestRunning => protoHackerApiManager.TestingStatus.Select(status => status is ApiTestStatus.Running);
}