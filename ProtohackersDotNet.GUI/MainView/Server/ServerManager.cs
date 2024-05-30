using System.Net.NetworkInformation;
using Microsoft.Extensions.Options;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public sealed partial class ServerManager : ObservableValidator, IStateSaveable<ServerManagerState>
{
    [ObservableProperty]
    IServer<IClient> server;

    public ObservableCollection<IServer<IClient>> Servers { get; }

    public SelectableEndPoint LocalEndPoint { get; }
    public TextEndPoint RemoteEndPoint { get; }

    readonly StartServerCommand startServerCommand;
    readonly ClearLogCommand clearLogsCommand;
    public TestServerCommand TestServerCommand { get; }

    public ServerManager(IEnumerable<IServer<IClient>> servers, IOptions<ServerManagerState> options,
        StartServerCommand startServerCommand, ClearLogCommand clearLogsCommand, TestServerCommand testServerCommand)
    {
        Servers = new(servers);
        this.server = Servers.FirstOrDefault(server => server.Name.Value == options.Value.Server, Servers.First());

        // this should trigger all our server descriptions to load lazily.
        _ = Task.WhenAll(Servers.Select(server => Task.Run(() => _ = server.Problem.Description)));

        var localIP = SystemIPs.FirstOrDefault(ip => ip.ToString() == options.Value?.LocalEndPoint?.IP, IPAddress.Any);
        LocalEndPoint = new(SystemIPs, localIP, options.Value?.LocalEndPoint?.Port);

        // no extra validation done here, because a null value is okay.
        _ = IPAddress.TryParse(options.Value?.RemoteEndPoint?.IP, out var ip);
        RemoteEndPoint = new(ip, options.Value?.RemoteEndPoint?.Port);

        this.startServerCommand = startServerCommand;
        this.clearLogsCommand = clearLogsCommand;
        TestServerCommand = testServerCommand;
    }

    partial void OnServerChanged(IServer<IClient> value) => this.clearLogsCommand.Clear();

    public void StartServer() => this.startServerCommand.StartServer(Server, LocalEndPoint.LatestValidEndPoint);

    public async Task StopServer() => _ = await Server.Stop();

    public void TestServer() => TestServerCommand.Test(Server, RemoteEndPoint.LatestValidEndPoint);

    public void RefreshLocalIPs()
    {
        LocalEndPoint.SelectableIPs.Clear();
        LocalEndPoint.SelectableIPs.AddRange(SystemIPs);
    }

    /// <summary>Called when the app exits. Save the current state out to json.</summary>
    public ServerManagerState GetState() => new() {
            LocalEndPoint = LocalEndPoint.ToSerializable(),
            RemoteEndPoint = RemoteEndPoint.ToSerializable(),
            Server = Server.Name.Value
        };

    static IEnumerable<IPAddress> SystemIPs
        => NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any);
}
