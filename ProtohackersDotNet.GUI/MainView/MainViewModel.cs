using System.Net.NetworkInformation;
using Microsoft.Extensions.Options;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.Servers.Interface.Server.Events;
using ProtoHackersDotNet.Servers.Helpers;

namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainViewModel : ObservableValidator
{
    [ObservableProperty]
    IServer<IClient> server;

    public ObservableCollection<IServer<IClient>> Servers { get; }

    public SelectableEndPoint LocalEndPoint { get; }
    public TextEndPointVM RemoteEndPoint { get; }

    public ClientManager ClientManager { get; }

    public MessageManager MessageManager { get; } = new();

    public MainViewModel(IEnumerable<IServer<IClient>> servers, IOptions<MainViewModelOptions> options, ProtoHackerApiManager testingManager, ClientManager clientManager)
    {
        Servers = new(servers);
        Server = Servers.FirstOrDefault(server => server.Name.Value == options.Value.Server, Servers.First());

        // this should trigger all our server descriptions to load lazily.
        _ = Task.WhenAll(Servers.Select(server => Task.Run(() => _ = server.Problem.Description)));

        var localIP = SystemIPs.FirstOrDefault(ip => ip.ToString() == options.Value?.LocalEndPoint?.IP, IPAddress.Any);
        LocalEndPoint = new(SystemIPs, localIP, options.Value?.LocalEndPoint?.Port);
        
        // no extra validation done here, because a null value is okay.
        _ = IPAddress.TryParse(options.Value?.RemoteEndPoint?.IP, out var ip);
        RemoteEndPoint = new(ip, options.Value?.RemoteEndPoint?.Port);

        ApiTestManager = testingManager;

        ClientManager = clientManager;
    }

    /// <summary>Called when the app exits. Save the current state out to json.</summary>
    public void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs args)
        => new MainViewModelOptions() {
            LocalEndPoint  = new(LocalEndPoint.IP?.ToString(),  LocalEndPoint.Port),
            RemoteEndPoint = new(RemoteEndPoint.IP?.ToString(), RemoteEndPoint.Port),
            Server = Server.Name.Value
        }.SaveSettings();

    #region Bound View Methods

    /// <summary>Starts the currently selected server.</summary>
    public void StartServer()
    {
        var serverEvents = Server.Start(LocalEndPoint.EndPoint);
        serverEvents.OfType<ClientConnectionEvent>().Subscribe(SubscribeClient, Stub.IgnoreError).DiscardDisposable();
        MessageManager.SubscribeToStream(serverEvents, MessageVM.FromSeverEvent, Server.Name.Value);
        serverEvents.Connect().DiscardDisposable();

        void SubscribeClient(ClientConnectionEvent clientEvent)
        {
            ClientManager.AddClient(clientEvent.Client);
            MessageManager.SubscribeToStream(clientEvent.Client.Events, MessageVM.FromClientEvent, 
                clientEvent.Client.ClientEndPoint.ToString());
        }
    }

    /// <summary>Stops the currently running server.</summary>
    /// <returns>A task that represents completion of the stop operation.</returns>
    public async Task StopServer() => _ = await Server.Stop();

    public ProtoHackerApiManager ApiTestManager { get; }

    public void Test()
    {
        var testEvents = ApiTestManager.TestServer(Server, RemoteEndPoint.EndPoint);
        MessageManager.SubscribeToStream(testEvents, MessageVM.FromTestEvent, "ProtoHackers Test API");
    }

    #endregion

    public void RefreshLocalIPs() => LocalEndPoint.SelectableIPs = new(SystemIPs);

    /// <summary>A list of operational IPs on the current system.</summary>
    static IEnumerable<IPAddress> SystemIPs
        => NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any);
}