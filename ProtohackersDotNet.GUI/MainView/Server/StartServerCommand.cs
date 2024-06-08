using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.EndPoint;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.Serialization;
using System.Net.NetworkInformation;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class StartServerCommand : IObservableCommand, IStateSaveable
{
    readonly ServerManager serverManager;
    readonly ClientManager clientManager;
    readonly MessageManager messageManager;

    public StartServerCommand(ServerManager serverManager, ClientManager clientManager, MessageManager messageManager,
                                StartServerCommandState state)
    {
        this.serverManager = serverManager;
        this.clientManager = clientManager;
        this.messageManager = messageManager;

        LocalEndPoint = new(
            ips: SystemIPs,
            ip: IPAddress.TryParse(state.LocalEndPoint?.IP, out var ip) ? ip : null,
            port: state.LocalEndPoint?.Port
        );
        CanExecute = Observable.CombineLatest(Executing, LocalEndPoint.Valid, (executing, valid) => !executing && valid)
                               .DistinctUntilChanged();
    }

    public IObservable<bool> CanExecute { get; }

    public IObservable<bool> Executing 
        => this.serverManager.Server.SelectMany(server => server?.Listening ?? Observable.Return(false));

    public void Execute() => Start();

    public SelectableEndPoint LocalEndPoint { get; }

    public void Start()
    {
        var server = this.serverManager.CurrentServer ?? ThrowArgumentNullException<IServer>();
        var localEndPoint = LocalEndPoint.LatestEndPoint ?? ThrowArgumentNullException<IPEndPoint>();
        Start(server, localEndPoint);
    }

    void Start(IServer server, IPEndPoint serverEndpoint)
    {
        var serverEvents = server.Start(serverEndpoint);
        serverEvents.OfType<ClientConnectionEvent>().Subscribe(SubscribeClient, Stub.IgnoreError).DiscardUnsubscribe();
        messageManager.SubscribeToStream(EventSource.FromServer(server, serverEvents));
        serverEvents.Connect().DiscardUnsubscribe();

        void SubscribeClient(ClientConnectionEvent clientEvent)
        {
            clientManager.AddClient(clientEvent.Client);
            messageManager.SubscribeToStream(EventSource.FromClient(clientEvent.Client, 
                clientEvent.Client.Events));
        }
    }

    static IEnumerable<IPAddress> SystemIPs
        => NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any);

    public IState GetState() => new StartServerCommandState() { LocalEndPoint = LocalEndPoint.ToSerializable() };
}