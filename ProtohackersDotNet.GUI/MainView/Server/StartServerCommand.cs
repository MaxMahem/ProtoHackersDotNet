using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.Servers.Interface.Server.Events;
using ProtoHackersDotNet.Servers.Helpers;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class StartServerCommand(ClientManager clientManager, MessageManager messageManager)
{
    public void StartServer(IServer server, IPEndPoint serverEndpoint)
    {
        var serverEvents = server.Start(serverEndpoint);
        serverEvents.OfType<ClientConnectionEvent>().Subscribe(SubscribeClient, Stub.IgnoreError).DiscardDisposable();
        messageManager.SubscribeToStream(serverEvents, MessageVM.FromSeverEvent, server.Name.Value);
        serverEvents.Connect().DiscardDisposable();

        void SubscribeClient(ClientConnectionEvent clientEvent)
        {
            clientManager.AddClient(clientEvent.Client);
            messageManager.SubscribeToStream(clientEvent.Client.Events, MessageVM.FromClientEvent,
                clientEvent.Client.ClientEndPoint.ToString(), clientEvent.Client.Server.LocalEndPoint!.ToString());
        }
    }
}