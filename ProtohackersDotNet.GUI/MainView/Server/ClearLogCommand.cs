using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.Messages;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class ClearLogCommand(ClientManager clientManager, MessageManager messageManager)
{
    public void Clear()
    {
        clientManager.ClearDisconnectedClients();
        messageManager.ClearMessages();
    }

    public void ClearMessages()
    {
        messageManager.ClearMessages();
    }
}
