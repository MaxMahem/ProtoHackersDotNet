using System.Collections.Concurrent;
using System.Reactive.Disposables;
using ProtoHackersDotNet.Helpers.ObservableTypes;
using IServerStatus = ProtoHackersDotNet.Servers.Interface.Server.ServerStatus;

namespace ProtoHackersDotNet.Servers.TcpServer;

public abstract class TcpServerBase<TClient> : IServer<TClient>
    where TClient : IClient
{
    CancellationTokenSource cancellationSource = new();
    readonly Subject<ServerEvent> serverEventObservable = new();
    readonly CompositeDisposable disposables;

    protected TcpServerBase() 
        => this.disposables = [this.cancellationSource, this.serverEventObservable, this.serverStatusObservable];

    #region Interface properties

    public abstract ServerName Name { get; }
    public abstract Problem Solution { get; }

    readonly ObservableValue<IServerStatus> serverStatusObservable = new(IServerStatus.Stopped);
    public IObservable<IServerStatus> ServerStatus => this.serverStatusObservable.Values;
    public IObservable<bool> Listening => ServerStatus.Select(status => status is IServerStatus.Listening)
                                                      .DistinctUntilChanged();

    public virtual IObservable<string?> Status => Observable.Return<string?>(null);

    public IPEndPoint? LocalEndPoint { get; private set; }

    #endregion

    #region Client Store

    /// <summary>Dictionary of currently active clients.</summary>
    readonly ConcurrentDictionary<Guid, TClient> activeClients = [];

    /// <summary>Enumeration of currently active <typeparamref name="TClient"/>.</summary>
    public IEnumerable<TClient> Clients => activeClients.Values;

    protected abstract TClient CreateClient(TcpClient client, CancellationToken token);

    #endregion

    #region Startup And Listening Methods

    public IConnectableObservable<ServerEvent> Start(IPEndPoint endPoint, CancellationToken token = default)
    {
        if (this.serverStatusObservable.LatestValue is IServerStatus.Listening) 
            ThrowInvalidOperationException("Server already started");
        this.cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        LocalEndPoint = endPoint;

        return Observable.Create<ServerEvent>(HandleSubscription).Publish();
    }

    async Task HandleSubscription(IObserver<ServerEvent> observer, CancellationToken unsubscribeToken)
    {
        Debug.Assert(this.cancellationSource is not null);
        var token = CTSHelper.LinkTokenSource(ref this.cancellationSource, unsubscribeToken);

        Debug.Assert(LocalEndPoint is not null);
        using TcpListener listener = new(LocalEndPoint);

        try {
            listener.Start();
            this.serverStatusObservable.LatestValue = IServerStatus.Listening;
            await OnStartup();
            observer.OnNext(ServerStatusChangeEvent.Startup(this));

            do {
                // wait here for new connection
                var newTcpConnection = await listener.AcceptTcpClientAsync(token);
                var connectedClient = CreateClient(newTcpConnection, token);
                this.activeClients[connectedClient.Id] = connectedClient;

                // subscribe to track completion of the client.
                connectedClient.Events.Finally(async () => await ClientDisconnect(connectedClient))
                               .Subscribe(Stub.DoNothing, Stub.IgnoreError, token);

                // this exposes the client externally so it may be subscribed to.
                observer.OnNext(new ClientConnectionEvent(this, connectedClient));
            } while (true); // only exit is via exception.
        } // operation canceled.
        catch (OperationCanceledException exception) when (exception.CancellationToken.IsCancellationRequested) {
            observer.OnNext(ServerStatusChangeEvent.Shutdown(this));
            observer.OnCompleted();
            this.serverStatusObservable.LatestValue = IServerStatus.Stopped;
        } // unhandled exception.
        catch (Exception exception) {
            observer.OnNext(ServerStatusChangeEvent.Terminated(this, exception));
            observer.OnError(exception);
            this.serverStatusObservable.LatestValue = IServerStatus.Terminated;
        }
        finally { // server shutdown
            DisposeClients();
            listener.Stop();
        }
    }

    async Task ClientDisconnect(TClient client)
    {
        await OnClientDisconnect(client);
        this.serverEventObservable.OnNext(new ClientDisconnectEvent(this, client));
        bool removed = this.activeClients.TryRemove(client.Id, out var _);
        Debug.Assert(removed);
    }

    #endregion

    #region Optional child override methods - these methods do nothing by default

    protected virtual Task OnStartup() => Task.CompletedTask;

    protected virtual Task OnClientDisconnect(TClient client) => Task.CompletedTask;
    
    #endregion

    /// <summary>Broadcasts <paramref name="message"/> to <paramref name="recipients"/>.</summary>
    public async Task Broadcast<TMessage>(IEnumerable<TClient> recipients, TMessage message)
        where TMessage : ITransmission
    {
        await Task.WhenAll(recipients.Select(client => client.Transmit(message)));
        this.serverEventObservable.OnNext(new ServerBroadcastEvent(this, message));
    }

    #region Shutdown and Dispose Methods

    public async Task<IDisposable> Stop()
    {
        if (this.serverStatusObservable.LatestValue is not IServerStatus.Listening)
            ThrowInvalidOperationException("Server not started");

        Debug.Assert(this.cancellationSource is not null);
        await this.cancellationSource.CancelAsync();

        return this;
    }

    void DisposeClients()
    {
        foreach (var client in this.activeClients.Values) client.Dispose();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        DisposeClients();
        this.cancellationSource?.Cancel();
        this.disposables.Dispose();
    }

    #endregion
}