using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.MainView.Server;
using ProtoHackersDotNet.GUI.Serialization;
using System.Reactive.Disposables;

namespace ProtoHackersDotNet.GUI.MainView;

public sealed class MainViewModel : IDisposable
{
    readonly CompositeDisposable disposables;

    public ServerManager ServerManager { get; }
    public ClientManager ClientManager { get; }
    public MessageManager MessageManager { get; }

    public MainViewModel(ServerManager serverManager, ClientManager clientManager, MessageManager messageManager,
        StateSaver stateSaver)
    {
        ServerManager = serverManager;
        ClientManager = clientManager;
        MessageManager = messageManager;

        this.disposables = [
            ClientManager,
            ServerManager.Server.Subscribe(stateSaver.Save),
            ServerManager.LocalEndPoint.Valid.Subscribe(stateSaver.Save),
            ServerManager.RemoteEndPoint.Valid.Subscribe(stateSaver.Save),
        ];
    }

    public void Dispose() => this.disposables.Dispose();
}