using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.Messages;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class ClearLogCommand(ClientManager clientManager, MessageManager messageManager)
{
    public void ClearClientsAndMessages()
    {
        clientManager.ClearDisconnectedClients();
        messageManager.ClearMessages();
    }
}
