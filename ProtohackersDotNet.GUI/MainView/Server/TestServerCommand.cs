using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class TestServerCommand(ProtoHackerApiManager apiManager, MessageManager messageManager)
{
    public void Test(IServer server, IPEndPoint remoteEndPoint)
    {
        var testEvents = apiManager.TestServer(server, remoteEndPoint);
        messageManager.SubscribeToStream(EventSource<TestEvent>.FromTestApi(apiManager, testEvents));
    }

    public IObservable<bool> TestRunning => apiManager.TestingStatus.Select(status => status is ApiTestStatus.Running);
}