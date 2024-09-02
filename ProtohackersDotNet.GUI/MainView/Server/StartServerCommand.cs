using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.EndPoint;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.Serialization;
using System.Net.NetworkInformation;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class ObservableCommand(Action execute, IObservable<bool> canExecute) : IObservableCommand
{
    public IObservable<bool> CanExecute => canExecute;

    public void Execute() => execute.Invoke();
}

public class StartServerCommand : IObservableCommand, IStateProvider
{
    readonly ServerManager serverManager;
    readonly ClientManager clientManager;
    readonly MessageManager messageManager;
    readonly StartServerCommandState state;

    public StartServerCommand(ServerManager serverManager, ClientManager clientManager, MessageManager messageManager,
                              StartServerCommandState state)
    {
        this.serverManager = serverManager;
        this.clientManager = clientManager;
        this.messageManager = messageManager;
        this.state = state;

        LocalEndPoint = new(
            ips: SystemIPs,
            ip: IPAddress.TryParse(state.LocalEndPoint?.IP, out var ip) ? ip : null,
            port: state.LocalEndPoint?.Port
        );

        CanExecute = Observable.CombineLatest(
            this.serverManager.ServerVM.Changes.SelectMany(server => server?.Server.Listening ?? Observable.Return(false)), 
            LocalEndPoint.EndPoint.Validity, 
            (executing, valid) => !executing && valid
        ).DistinctUntilChanged();
    }

    public IObservable<bool> CanExecute { get; }

    public void Execute()
    {
        var server = this.serverManager.ServerVM.Value?.Server ?? ThrowArgumentNullException<IServer>();
        var localEndPoint = LocalEndPoint.EndPoint.Value ?? ThrowArgumentNullException<IPEndPoint>();

        var serverEvents = server.Start(localEndPoint);
        serverEvents.OfType<ClientConnectionEvent>().Subscribe(SubscribeClient, Stub.IgnoreError).DiscardUnsubscribe();
        this.messageManager.SubscribeToStream(EventSource.FromServer(server, serverEvents));
        serverEvents.Connect().DiscardUnsubscribe();

        void SubscribeClient(ClientConnectionEvent clientEvent)
        {
            this.clientManager.AddClient(clientEvent.Client);
            var eventSource = EventSource.FromClient(clientEvent.Client, clientEvent.Client.Events);
            this.messageManager.SubscribeToStream(eventSource);
        }
    }

    public SelectableEndPoint LocalEndPoint { get; }

    static IEnumerable<IPAddress> SystemIPs
        => NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any);

    public IObservable<IState> StateChanges => LocalEndPoint.StateChanges.Select(this.state.Update);
}