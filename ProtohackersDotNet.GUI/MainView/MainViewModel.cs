using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using Avalonia.Threading;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Specialized;

namespace ProtoHackersDotNet.GUI.MainView;

public partial class MainViewModel : ObservableValidator
{
    public IServerFactory ServerFactory { get; } = AvaliableServers.First();

    public IPAddress ServerIP { get; } = IPAddress.Any;

    [ObservableProperty, NotifyPropertyChangedFor(nameof(CanServerStart))]
    ushort? serverPort = 0;

    [ObservableProperty]
    bool loggingEnabled = false;

    [ObservableProperty]
    string logFileName = string.Empty;

    public IObservable<int> ActiveClientCount { get; }

    // [ObservableProperty, NotifyPropertyChangedFor(nameof(ClientHeader))]
    // int activeClientCount = 0;

    FileStream? logFileStream = null;
    StreamWriter? logStream = null;

    IServer<IClient>? server = null;
    

    readonly ObservableCollection<FormatedMessage> messages = [];
    public FlatTreeDataGridSource<FormatedMessage>? Messages { get; }

    readonly ObservableCollection<ClientVM> clients = [];
    public FlatTreeDataGridSource<ClientVM>? Clients { get; }

    #region Constructors

    public MainViewModel()
    {
        _ = this.TestSubmissionClient.DefaultRequestHeaders.UserAgent.TryParseAdd(UserAgent)
                || ThrowHelper.ThrowInvalidOperationException<bool>("User agent parse failed.");

        // build an observer that triggers a client count when a client is added/removed or when the connection status changes
        ActiveClientCount = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
            handler => this.clients.CollectionChanged += handler,
            handler => this.clients.CollectionChanged -= handler)
            .SelectMany(_ => this.clients.Select(vm => vm.Client.ConnectionStatusChanges).Merge())
            .Select(_ => this.clients.Where(vm => vm.Client.ConnectionStatus is ConnectionStatus.Connected).Count())
            .StartWith(this.clients.Count);

        var star1 = new GridLength(1, GridUnitType.Star);
        var star2 = new GridLength(2, GridUnitType.Star);
        var star4 = new GridLength(4, GridUnitType.Star);

        Messages = new FlatTreeDataGridSource<FormatedMessage>(this.messages) {
            Columns = {
                new TextColumn<FormatedMessage, string>("Source", message => message.Source),
                new TemplateColumn<FormatedMessage>("Timestamp", "TimestampCell"),
                new TextColumn<FormatedMessage, string>("Type", message => message.Type),
                new TextColumn<FormatedMessage, string>("Message", message => message.Message.TrimEnd(), star4),
                // new TemplateColumn<FormatedMessage>("Message", "MessageCell", fourStarLength),
            }
        };
        
        Clients = new FlatTreeDataGridSource<ClientVM>(this.clients) {
            Columns = {
                new TemplateColumn<ClientVM>("Source", "EndPointCell", null, star2),
                new TemplateColumn<ClientVM>("Status", "StatusCell", null, star1),
                new TemplateColumn<ClientVM>("Age", "ConnectionAgeCell", null, star1),
                new TemplateColumn<ClientVM>("Recieved", "RecievedCell", null, star1),
                new TemplateColumn<ClientVM>("Transmitted", "TransmittedCell", null, star1),
                // new TextColumn<IClient, IObservable<string>>("Status", client => client.Status, fourStarLength),
            }
        };
    }

    public MainViewModel(MainViewModelSettings? serialization) : this()
    {
        ServerFactory = AvaliableServers.FirstOrDefault(server => server.Name == serialization?.Server, AvaliableServers.First());
        ServerIP = SystemIPs.FirstOrDefault(ip => ip.ToString() == serialization?.IP, IPAddress.Any);
        this.serverPort = serialization?.Port ?? 0;
    }

    #endregion

    public void PostMessage(FormatedMessage message)
    {
        this.messages.Add(message);
        if (LoggingEnabled) {
            this.logStream?.WriteLine($"{message.Source}, {message.Timestamp:s}, {message.Message}");
            this.logStream?.Flush();
        }
    }

    #region On Methods

    public void OnRemoteConnect(object? sender, NewClient message)
    {
        this.clients.Add(new(message.Client));
        PostMessage(message.ToFormated());

        message.Client.DataTransmitted += OnDataTransmitted;
        message.Client.DataRecieved += OnDataRecieved;
        message.Client.Exception += OnClientException;
    }

    public void OnRemoteDisconnect(object? sender, RemoteDisconnect message)
    {
        PostMessage(message.ToFormated());

        message.Client.DataTransmitted -= OnDataTransmitted;
        message.Client.DataRecieved -= OnDataRecieved;
        message.Client.Exception -= OnClientException;
    }

    public void OnDataTransmitted(object? sender, DataTransmission transmission)
    {
        if (transmission.Broadcast) return;
        PostMessage(transmission.ToFormated());
    }

    public void OnDataRecieved(object? sender, DataReciept reciept) => PostMessage(reciept.ToFormated());

    public void OnClientException(object? sender, Exception exception)
    {
        string source = sender is IClient client ? client.RemoteEndPoint?.ToString() ?? string.Empty : string.Empty;
        PostMessage(new() {
            Source = source,
            Message = exception.Message,
            Timestamp = DateTime.UtcNow.ToString("HH:mm:ss.ffff"),
            Type = "Exception",
        });
    }

    public void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs args)
        => new MainViewModelSettings(this).SaveSettings();

    #endregion

    #region Bound View Methods

    public void ClearMessages() => this.messages.Clear();
    public void ClearClients()
    {
        for (int index = this.clients.Count - 1; index >= 0; index--)
            if (this.clients[index].Client.ConnectionStatus is not ConnectionStatus.Connected) 
                this.clients.RemoveAt(index);
    }

    public async Task StartServer() {
        if (ServerPort is null) throw new NullReferenceException();

        if (LoggingEnabled) {
            try {
                LogFileName = Path.GetFullPath($"{ServerFactory.Name}-{DateTime.Now:yyyyMMdd-HHmmss}.log");
                this.logFileStream = File.Create(LogFileName);
                this.logStream = new StreamWriter(this.logFileStream);
            }
            catch (Exception exception) {
                LogFileName = $"{exception.Message}";
                LoggingEnabled = false;
            }
        }

        this.server = ServerFactory.Create(ServerIP, ServerPort.Value);
        this.server.RemoteConnect    += OnRemoteConnect;
        this.server.RemoteDisconnect += OnRemoteDisconnect;
        this.server.ServerEvent      += (sender, message) => Dispatcher.UIThread.Post(() => PostMessage(message.ToFormated()));
        // this.server.ClientEvent  += (sender, message) => Dispatcher.UIThread.Post(() => HandleClientMessage(sender, message));

        var serverTask = this.server.Start().ConfigureAwait(false);

        OnPropertyChanged(nameof(ServerRunning));
        OnPropertyChanged(nameof(CanServerStart));

        await serverTask;
        // PostMessage(this, new(null, DateTime.Now, EndpointMessageType.SystemStart, SelectedServer.Name));
    }

    public async Task StopServer() {
        ArgumentNullException.ThrowIfNull(this.server);

        await this.server.Stop();

        this.server.Dispose();
        this.server = null;

        this.logStream?.Flush();
        this.logStream?.Dispose();
        this.logFileStream?.Dispose();

        OnPropertyChanged(nameof(ServerRunning));
        OnPropertyChanged(nameof(CanServerStart));
    }

    readonly HttpClient TestSubmissionClient = new();
    static readonly string UserAgent = $"{App.AppName}/{App.Version}";
    static readonly Uri TestSubmissionUrl = new("https://api.protohackers.com/submit");

    public async Task Test()
    {
        TestRequest request = new(){
            Problem = ServerFactory.ProblemId,
            IpAddress = IPAddress.Parse("160.2.91.27").ToString(),
            Port = ServerPort!.Value,
        };

        var result = await TestSubmissionClient .PostAsJsonAsync(TestSubmissionUrl, request, TestRequestGenerationContext.Default.TestRequest);
    }

    #endregion

    public bool CanServerStart => !ServerRunning && ServerPort is not null;
    public bool ServerRunning => this.server?.Running ?? false;

    public static List<IServerFactory> AvaliableServers { get; } = [.. ServerFactories.Avaliable];

    public static IEnumerable<IPAddress> SystemIPs 
        => NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any);
}

[JsonSerializable(typeof(MainViewModelSettings))]
internal partial class MainVMGenerationContext : JsonSerializerContext;

[JsonSerializable(typeof(TestRequest))]
[JsonSourceGenerationOptions(NumberHandling = JsonNumberHandling.WriteAsString, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class TestRequestGenerationContext : JsonSerializerContext;
