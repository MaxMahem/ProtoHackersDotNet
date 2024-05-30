using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.MainView.Server;

namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainViewModel(ServerManager serverManager, ClientManager clientManager, MessageManager messageManager) 
{
    public ServerManager ServerManager { get; } = serverManager;

    public ClientManager ClientManager { get; } = clientManager;

    public MessageManager MessageManager { get; } = messageManager;
}