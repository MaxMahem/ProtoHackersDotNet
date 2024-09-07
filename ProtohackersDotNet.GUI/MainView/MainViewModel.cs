using ProtoHackersDotNet.GUI.Helpers;
using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.EndPoint;
using ProtoHackersDotNet.GUI.MainView.Grader;
using ProtoHackersDotNet.GUI.MainView.IpIfyClient;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.MainView.Server;
using ProtoHackersDotNet.GUI.Serialization;
using System.Net.NetworkInformation;
namespace ProtoHackersDotNet.GUI.MainView;

public sealed class MainViewModel : IStateProvider
{
    public MainViewModel(ServerManager serverManager, ClientManager clientManager, MessageManager messageManager,
        GradingService gradingService, IpIfyClient.IpIfyClient ipIfyClient,
        MainViewModelState mainViewModelState)
    {
        ServerManager = serverManager;
        ClientManager = clientManager;
        MessageManager = messageManager;
        GradingService = gradingService;

        LocalEndPoint = new(SystemIPs, mainViewModelState.LocalEndPoint);
        ExternalEndPoint = new(mainViewModelState.ExternalEndPoint);

        StateChanges = Observable.CombineLatest(LocalEndPoint.StateChanges, ExternalEndPoint.StateChanges, mainViewModelState.Update);

        StartServerCommand = new(serverManager.ServerVM, LocalEndPoint);
        StartServerCommand.Results.Subscribe(OnStartServer).DiscardUnsubscribe();
        
        TestServerCommand  = new(serverManager.ServerVM, ExternalEndPoint, gradingService);
        TestServerCommand.Results.Subscribe(OnTestServer).DiscardUnsubscribe();

        GetIpCommand = new ObservableAsyncCommand<IPAddress>(ipIfyClient.GetExternalIP, Observable.Return(true), true);
        GetIpCommand.Results.Subscribe(ip => ExternalEndPoint.IP.Value = ip).DiscardUnsubscribe(); 

        // When the server changes, clear the logs.
        serverManager.ServerVM.Changes.Select(_ => Unit.Default).Subscribe(ClearClientsAndMessages).DiscardUnsubscribe();
    }

    void OnStartServer(StartServerResult result)
    {
        result.Events.OfType<ClientConnectionEvent>().Subscribe(SubscribeClient, Stub.IgnoreError).DiscardUnsubscribe();
        MessageManager.SubscribeToStream(EventSource.FromServer(result.Server, result.Events));
        result.Events.Connect().DiscardUnsubscribe();

        void SubscribeClient(ClientConnectionEvent clientEvent)
        {
            ClientManager.AddClient(clientEvent.Client);
            var eventSource = EventSource.FromClient(clientEvent.Client, clientEvent.Client.Events);
            MessageManager.SubscribeToStream(eventSource);
        }
    }

    void OnTestServer(TestServerResult result)
    {
        MessageManager.SubscribeToStream(EventSource.FromGrader(GradingService, result.GradingEvents));
        result.ServerVM.ObserveTest(result.GradingEvents);
    }

    public ServerManager ServerManager { get; }
    public ClientManager ClientManager { get; }
    public MessageManager MessageManager { get; }
    public GradingService GradingService { get; }

    
    public SelectableEndPoint LocalEndPoint { get; }
    public TextEndPoint ExternalEndPoint { get; }

    public StartServerCommand StartServerCommand { get; }
    public TestServerCommand TestServerCommand { get; }
    public IObservableCommand<IPAddress> GetIpCommand { get; }

    public IObservable<IState> StateChanges { get; }

    public static IEnumerable<IPAddress> SystemIPs
        => NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any);

    void ClearClientsAndMessages(Unit _)
    {
        ClientManager.ClearDisconnectedClients();
        MessageManager.ClearMessages();
    }
}

public class MainViewModelState : IState
{
    [JsonIgnore] public string ObjectName => nameof(MainViewModelState);

    public SerializableEndPoint LocalEndPoint { get; set; } = new();
    public SerializableEndPoint ExternalEndPoint { get; set; } = new();

    public MainViewModelState Update(SerializableEndPoint localEndPoint, SerializableEndPoint externalEndPoint)
    {
        LocalEndPoint = localEndPoint;
        ExternalEndPoint = externalEndPoint;
        return this;
    }
}