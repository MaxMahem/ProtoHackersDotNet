using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Xml.Linq;
using static CommunityToolkit.Diagnostics.ThrowHelper;

namespace ProtoHackersDotNet.Servers.TcpServer;

abstract public class TcpServerBase<TClient> : IServer<TClient>
    where TClient : IClient
{
    TcpListener? listener;
    CancellationTokenSource? cancellationTokenSource;

    public abstract string Name { get; }
    public abstract int ProblemId { get; }

    public string Description => descriptionCache.Value;
    readonly Lazy<string> descriptionCache;

    public TcpServerBase()
    {
        this.descriptionCache = new(() => File.ReadAllText(Path.Combine(Name, Name + ".md")));
    }

    public IPEndPoint? EndPoint => this.listener?.LocalEndpoint as IPEndPoint;

    protected abstract TClient CreateClient(TcpClient client, CancellationToken token);

    readonly Dictionary<Task, TClient> taskClients = [];

    /// <summary>Enumeration of currently active <typeparamref name="TClient"/>.</summary>
    public IEnumerable<TClient> Clients => taskClients.Values;

    #region Observables

    readonly BehaviorSubject<bool> listerningObserver = new(false);
    public IObservable<bool> Listening => this.listerningObserver.AsObservable();
    public bool CurrentlyListening {
        get => this.listerningObserver.Value;
        private set => this.listerningObserver.OnNext(value);
    }

    public IObservable<TClient> ClientConnections => clientConnectionObserver.AsObservable();
    readonly Subject<TClient> clientConnectionObserver = new();

    public IObservable<TClient> ClientDisconnections => clientDisconnetionObserver.AsObservable();
    readonly Subject<TClient> clientDisconnetionObserver = new();

    public IObservable<ITransmission> Broadcasts => broadcastObserver.AsObservable();
    readonly Subject<ITransmission> broadcastObserver = new();

    public IObservable<Exception> ServerExceptions => serverExceptionObserver.AsObservable();
    readonly Subject<Exception> serverExceptionObserver = new();

    #endregion

    /// <summary>Starts the server and begins listening for clients.</summary>
    /// <param name="token">A cancellation token that can be used to halt the server.</param>
    /// <returns>A task that represents completion of the listening process.</returns>
    public async Task Start(IPEndPoint endPoint, CancellationToken? token = null)
    {
        if (CurrentlyListening) ThrowInvalidOperationException("Server already started.");
        this.cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token ?? new CancellationToken());

        this.listener = new(endPoint);
        this.listener.Start();
        CurrentlyListening = true;
        
        // NotifyServerEvent(ServerEventType.Start, $"{Name} started.");

        Task<TcpClient> listenerTask = this.listener.AcceptTcpClientAsync(this.cancellationTokenSource.Token).AsTask();

        var tasks = this.taskClients.Keys.Append(listenerTask);

        try {
            do {
                var completedTask = await Task.WhenAny(tasks);

                switch (completedTask) {
                    // client connect.
                    case Task<TcpClient> completedListenerTask:
                        var client = CreateClient(completedListenerTask.Result, cancellationTokenSource.Token);

                        this.clientConnectionObserver.OnNext(client);

                        // start the client handler and add the managing task to the dictionary.
                        var clientTask = client.HandleClient();
                        this.taskClients[clientTask] = client;

                        // reset the listner.
                        listenerTask = this.listener.AcceptTcpClientAsync(cancellationTokenSource.Token).AsTask();
                        break;
                    // client disconnect.
                    case Task completedClientTask:
                        var completedClient = this.taskClients[completedTask];

                        this.clientDisconnetionObserver.OnNext(completedClient);

                        completedClient.Dispose();

                        _ = this.taskClients.Remove(completedTask) || ThrowInvalidOperationException<bool>("Task not found in dictionary.");
                        break;
                    default: throw new InvalidOperationException();
                }
                tasks = this.taskClients.Keys.Append(listenerTask);
            } while (!this.cancellationTokenSource.Token.IsCancellationRequested);
        }
        catch (Exception exception) {
            this.serverExceptionObserver.OnNext(exception);
        }
        finally {
            DisposeClients();
            this.listener.Stop();
            CurrentlyListening = false;
        }
    }

    /// <summary>Broadcasts <paramref name="message"/> to <paramref name="recepients"/>.</summary>
    public async Task Broadcast<TMessage>(IEnumerable<TClient> recepients, TMessage message)
        where TMessage : ITransmission
    {
        await Task.WhenAll(recepients.Select(client => client.Transmit(message)));
        this.broadcastObserver.OnNext(message);
    }

    void DisposeClients()
    {
        foreach (var client in this.taskClients.Values) client.Dispose();
    }

    public async Task Stop()
    {
        if (!CurrentlyListening) ThrowInvalidOperationException("Server not started.");
        ArgumentNullException.ThrowIfNull(this.cancellationTokenSource);
        ArgumentNullException.ThrowIfNull(this.listener);

        await this.cancellationTokenSource.CancelAsync();
        DisposeClients();

        this.listener?.Stop();
        CurrentlyListening = false;
        // NotifyServerEvent(ServerEventType.Stop, $"{Name} stopped.");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DisposeClients();
        this.cancellationTokenSource?.Cancel();
        this.cancellationTokenSource?.Dispose();
        this.listener?.Dispose();
    }
}