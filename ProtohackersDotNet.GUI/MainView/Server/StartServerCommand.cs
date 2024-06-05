using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.Messages;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class StartServerCommand(ClientManager clientManager, MessageManager messageManager)
{
    public void StartServer(IServer server, IPEndPoint serverEndpoint)
    {
        var serverEvents = server.Start(serverEndpoint);
        serverEvents.OfType<ClientConnectionEvent>().Subscribe(SubscribeClient, Stub.IgnoreError).DiscardDisposable();
        messageManager.SubscribeToStream(EventSource.FromServer(server, serverEvents));
        serverEvents.Connect().DiscardDisposable();

        void SubscribeClient(ClientConnectionEvent clientEvent)
        {
            clientManager.AddClient(clientEvent.Client);
            messageManager.SubscribeToStream(EventSource.FromClient(clientEvent.Client, 
                clientEvent.Client.Events));
        }
    }
}