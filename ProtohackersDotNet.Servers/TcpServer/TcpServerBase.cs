using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Disposables;
using ProtoHackersDotNet.Helpers.ObservableTypes;
using IServerStatus = ProtoHackersDotNet.Servers.Interface.Server.ServerStatus;

namespace ProtoHackersDotNet.Servers.TcpServer;

public abstract class TcpServerBase<TClient> : IServer<TClient>
    where TClient : IClient
{
    CancellationTokenSource cancellationSource = new();
    readonly CompositeDisposable disposables;

    protected TcpServerBase() 
        => this.disposables = [this.cancellationSource, ServerEventObservable, this.serverStatusObservable];

    protected Subject<IEvent> ServerEventObservable { get; } = new();

    #region Interface properties

    public abstract ServerName Name { get; }
    public abstract Problem Solution { get; }

    readonly ObservableValue<IServerStatus> serverStatusObservable = new(IServerStatus.Stopped);
    public IObservable<IServerStatus> ServerStatus => this.serverStatusObservable.Value;
    public IObservable<bool> Listening => ServerStatus.Select(status => status is IServerStatus.Listening)
                                                      .DistinctUntilChanged();
    public bool CurrentlyListening => this.serverStatusObservable.CurrentValue is IServerStatus.Listening;

    public virtual IObservable<string?> Status => Observable.Return<string?>(null);

    public IPEndPoint? LocalEndPoint { get; private set; }

    #endregion

    #region Client Store

    /// <summary>Dictionary of currently active clients.</summary>
    readonly ConcurrentDictionary<TClient, Unit> activeClients = [];

    /// <summary>Enumeration of currently active <typeparamref name="TClient"/>.</summary>
    public IEnumerable<TClient> Clients => activeClients.Keys;

    protected abstract TClient CreateClient(TcpClient client, CancellationToken token);

    #endregion

    #region Startup And Listening Methods

    public IConnectableObservable<IEvent> Start(IPEndPoint endPoint, CancellationToken token = default)
    {
        if (this.serverStatusObservable.CurrentValue is IServerStatus.Listening) 
            ThrowInvalidOperationException("Server already started");
        this.cancellationSource.Dispose();
        this.cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        LocalEndPoint = endPoint;

        return Observable.Create<IEvent>(HandleSubscription).Merge(ServerEventObservable)
                         .Publish();
    }

    async Task HandleSubscription(IObserver<IEvent> observer, CancellationToken unsubscribeToken)
    {
        Debug.Assert(this.cancellationSource is not null);
        var token = CTSHelper.LinkTokenSource(ref this.cancellationSource, unsubscribeToken);

        Debug.Assert(LocalEndPoint is not null);
        using TcpListener listener = new(LocalEndPoint);

        try {
            listener.Start();
            this.serverStatusObservable.CurrentValue = IServerStatus.Listening;
            await OnStartup();
            observer.OnNext(new ServerStartupEvent(this));

            do {// wait here for new connection
                var newTcpConnection = await listener.AcceptTcpClientAsync(token);
                var connectedClient = CreateClient(newTcpConnection, token);
                this.activeClients[connectedClient] = Unit.Default;

                // this subscription track completion of the client
                connectedClient.Events.Finally(async () => await ClientDisconnect(connectedClient))
                               .Subscribe(Stub.DoNothing, Stub.IgnoreError, token);

                // this exposes the client externally
                observer.OnNext(new ClientConnectionEvent(this, connectedClient));
            } while (true); // only exit is via exception/cancel.
        } // operation canceled.
        catch (OperationCanceledException exception) when (exception.CancellationToken.IsCancellationRequested) {
            observer.OnNext(new ServerShutdownEvent(this));
            observer.OnCompleted();
            ServerEventObservable.OnCompleted(); // to complete the merged observable must complete both observables
            this.serverStatusObservable.CurrentValue = IServerStatus.Stopped;
        } // unhandled exception.
        catch (Exception exception) {
            observer.OnNext(new ServerTerminatedEvent(this, exception));
            observer.OnError(exception);
            this.serverStatusObservable.CurrentValue = IServerStatus.Terminated;
        }
        finally { // server shutdown
            DisposeClients();
            listener.Stop();
        }
    }

    async Task ClientDisconnect(TClient client)
    {
        await OnClientDisconnect(client);
        ServerEventObservable.OnNext(new ClientDisconnectEvent(this, client));
        bool removed = this.activeClients.TryRemove(client, out var _);
        Debug.Assert(removed);
    }

    #endregion

    #region Optional child override methods - these methods do nothing by default

    protected virtual Task OnStartup() => Task.CompletedTask;

    protected virtual Task OnClientDisconnect(TClient client) => Task.CompletedTask;
    
    #endregion

    #region Shutdown and Dispose Methods

    public async Task<IDisposable> Stop()
    {
        if (this.serverStatusObservable.CurrentValue is not IServerStatus.Listening)
            ThrowInvalidOperationException("Server not started");

        Debug.Assert(this.cancellationSource is not null);
        await this.cancellationSource.CancelAsync();

        return this;
    }

    void DisposeClients()
    {
        foreach (var client in this.activeClients.Keys) client.Dispose();
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