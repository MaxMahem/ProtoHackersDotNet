using System.Net.NetworkInformation;
using CommunityToolkit.Mvvm.ComponentModel;
using ProtoHackersDotNet.GUI.Serialization;
using ProtoHackersDotNet.GUI.EndPointVM;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public sealed class ServerManager : IStateSaveable
{
    readonly ObservableValue<IServer<IClient>> serverObserver;

    public IServer<IClient> ServerValue {
        get => this.serverObserver.LatestValue;
        set => this.serverObserver.LatestValue = value;
    }

    /// <summary>Provides updates to the value of <see cref="ServerManager.ServerValue"/> 
    /// as it changes.</summary>
    public IObservable<IServer<IClient>> Server => this.serverObserver.Values;
    
    public ObservableCollection<IServer<IClient>> Servers { get; }

    public SelectableEndPoint LocalEndPoint { get; }
    public TextEndPoint RemoteEndPoint { get; }

    readonly StartServerCommand startServerCommand;
    public TestServerCommand TestServerCommand { get; }

    public ServerManager(IEnumerable<IServer<IClient>> servers, ServerManagerState options,
        StartServerCommand startServerCommand, ClearLogCommand clearLogsCommand, TestServerCommand testServerCommand)
    {
        Servers = new(servers);
        var initialServer = Servers.FirstOrDefault(server => server.Name.Value == options.Server, Servers.First());
        this.serverObserver = new(initialServer);

        // this should trigger all our server descriptions to load lazily.
        _ = Task.WhenAll(Servers.Select(server => Task.Run(() => _ = server.Solution.Description)));

        var localIP = SystemIPs.FirstOrDefault(ip => ip.ToString() == options.LocalEndPoint?.IP, IPAddress.Any);
        LocalEndPoint = new(SystemIPs, localIP, options.LocalEndPoint?.Port);

        // no extra validation done here, because a null value is okay.
        _ = IPAddress.TryParse(options.RemoteEndPoint?.IP, out var ip);
        RemoteEndPoint = new(ip, options.RemoteEndPoint?.Port);

        // When the server changes, clear the logs.
        Server.Subscribe(_ => clearLogsCommand.ClearClientsAndMessages()).DiscardDisposable();

        this.startServerCommand = startServerCommand;
        TestServerCommand = testServerCommand;
    }

    public void StartServer() => this.startServerCommand.StartServer(serverObserver.LatestValue, LocalEndPoint.LatestValidEndPoint);

    public async Task StopServer() => _ = await serverObserver.LatestValue.Stop();

    public void TestServer() => TestServerCommand.Test(serverObserver.LatestValue, RemoteEndPoint.LatestValidEndPoint);

    public void RefreshLocalIPs()
    {
        LocalEndPoint.SelectableIPs.Clear();
        LocalEndPoint.SelectableIPs.AddRange(SystemIPs);
    }

    /// <summary>Called when the app exits. Save the current state out to json.</summary>
    public IState GetState() => new ServerManagerState() {
            LocalEndPoint = LocalEndPoint.ToSerializable(),
            RemoteEndPoint = RemoteEndPoint.ToSerializable(),
            Server = serverObserver.LatestValue.Name.Value
        };

    static IEnumerable<IPAddress> SystemIPs
        => NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any);
}
