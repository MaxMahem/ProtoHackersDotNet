using System.Threading.Tasks;
using System.Net.NetworkInformation;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Specialized;
using ProtoHackersDotNet.GUI.MainView.ApiTest;
using Microsoft.Extensions.Options;
using ProtoHackersDotNet.Servers.Interfaces;
using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.GUI.MainView.Client.Messages;

namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainViewModel : ObservableValidator
{
    [ObservableProperty]
    IServer<IClient> server;

    public ObservableCollection<IServer<IClient>> Servers { get; }

    public SelectableEndPoint LocalEndPoint { get; }
    public TextEndPointVM RemoteEndPoint { get; }

    //[ObservableProperty]
    //public IPAddress localIP = IPAddress.Any;

    // const ushort DEFAULT_PORT_NUMBER = 0;
    //ushort localPort = DEFAULT_PORT_NUMBER;
    //public ushort? LocalPort { 
    //    get => this.localPort;
    //    set => this.localPort = value ?? DEFAULT_PORT_NUMBER;
    //}

    //IPEndPoint LocalEndPoint => new IPEndPoint(LocalIP, this.localPort);

    [ObservableProperty]
    bool loggingEnabled = false;

    [ObservableProperty]
    string logFileName = string.Empty;

    FileStream? logFileStream;
    StreamWriter? logStream;

    public IObservable<int> MessageCount 
        => Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                         handler => Messages.CollectionChanged += handler,
                         handler => Messages.CollectionChanged -= handler)
                     .Select(_ => Messages.Count)
                     .StartWith(Messages.Count);

    public ObservableCollection<DisplayMessage> Messages { get; } = [];

    public ClientManager ClientManager { get; }

    #region Constructors

    public MainViewModel(IEnumerable<IServer<IClient>> servers, IOptions<MainViewModelOptions> options, ApiTestManager testingManager, ClientManager clientManager)
    {
        Servers = new(servers);
        Server = Servers.FirstOrDefault(server => server.Name == options.Value.Server, Servers.First());

        var localIP = SystemIPs.FirstOrDefault(ip => ip.ToString() == options.Value?.LocalEndPoint?.IP, IPAddress.Any);
        LocalEndPoint = new(SystemIPs, localIP, options.Value?.LocalEndPoint?.Port);
        
        // no extra validation done here, because a null value is okay.
        _ = IPAddress.TryParse(options.Value?.RemoteEndPoint?.IP, out var ip);
        RemoteEndPoint = new(ip, options.Value?.RemoteEndPoint?.Port);

        ApiTestManager = testingManager;

        ClientManager = clientManager;
    }

    #endregion

    public void PostMessage(IDisplayMessage message)
    {
        Messages.Add(message.ToDisplayMessage());
        if (LoggingEnabled) {
            this.logStream?.WriteLine($"{message.Source}, {message.Timestamp:s}, {message.Message}");
            this.logStream?.Flush();
        }
    }

    /// <summary>Called when the app exits. Save the current state out to json.</summary>
    public void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs args)
        => new MainViewModelOptions() {
            LocalEndPoint = new(LocalEndPoint.IP?.ToString(), LocalEndPoint.Port),
            RemoteEndPoint = new(RemoteEndPoint.IP?.ToString(), RemoteEndPoint.Port),
            Server = Server.Name
        }.SaveSettings();

    #region Bound View Methods

    public void ClearMessages() => Messages.Clear();

    public async Task StartServer()
    {
        if (LoggingEnabled) {
            try {
                LogFileName = Path.GetFullPath($"{Server.Name}-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                this.logFileStream = File.Create(LogFileName);
                this.logStream = new StreamWriter(this.logFileStream);
            }
            catch (Exception exception) {
                LogFileName = $"{exception.Message}";
                LoggingEnabled = false;
            }
        }

        using var clientAdd     = Server.ClientConnections.Subscribe(ClientManager.AddClient);
        using var clientPost    = Server.ClientConnections.Subscribe(client => PostMessage(new ClientConnection(Server, client)));
        using var clientPart    = Server.ClientDisconnections.Subscribe(client => PostMessage(new ClientDisconnection(Server, client)));
        using var clientMessage = ClientManager.ClientMessages.Subscribe(PostMessage);

        var serverTask = Server.Start(LocalEndPoint.EndPoint).ConfigureAwait(false);

        // PostMessage(this, new(null, DateTime.Now, EndPointMessageType.SystemStart, SelectedServer.Name));
        await serverTask;
    }

    public async Task StopServer()
    {
        await Server.Stop();
        Server.Dispose();

        this.logStream?.Flush();
        this.logStream?.Dispose();
        this.logFileStream?.Dispose();
    }

    public ApiTestManager ApiTestManager { get; }

    public async Task Test()
    {
        using var testingDisposable = ApiTestManager.TestingEvents.Subscribe(PostMessage);

        await ApiTestManager.CheckProblem(Server, RemoteEndPoint.EndPoint);
    }

    #endregion

    static IEnumerable<IPAddress> SystemIPs
        => NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any);
}
