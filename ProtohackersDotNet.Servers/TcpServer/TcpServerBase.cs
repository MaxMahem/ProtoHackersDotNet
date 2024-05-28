using System.Collections.Concurrent;
using static CommunityToolkit.Diagnostics.ThrowHelper;

namespace ProtoHackersDotNet.Servers.TcpServer;

abstract public class TcpServerBase<TClient> : IServer<TClient>
    where TClient : IClient
{
    CancellationTokenSource? cancellationSource;

    public abstract ServerName Name { get; }
    public abstract Problem Problem { get; }

    public IPEndPoint? LocalEndPoint { get; private set; }

    protected abstract TClient CreateClient(TcpClient client, CancellationToken token);

    /// <summary>Dictionary of currently active clients.</summary>
    readonly ConcurrentDictionary<Guid, TClient> activeClients = [];

    /// <summary>Temporary observer reference for pushing notifications while the server is listening.</summary>
    IObserver<ServerEvent>? serverObserver;

    /// <summary>Enumeration of currently active <typeparamref name="TClient"/>.</summary>
    public IEnumerable<TClient> Clients => activeClients.Values;

    readonly BehaviorSubject<bool> listeningObserver = new(false);
    public IObservable<bool> Listening => this.listeningObserver.AsObservable();
    public bool CurrentlyListening {
        get         => this.listeningObserver.Value;
        private set => this.listeningObserver.OnNext(value);
    }

    public IConnectableObservable<ServerEvent> Start(IPEndPoint endPoint, CancellationToken token = default)
    {
        if (CurrentlyListening) ThrowInvalidOperationException("Server already started");
        this.cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        LocalEndPoint = endPoint;

        return Observable.Create<ServerEvent>(HandleSubscription).Publish();
    }

    async Task HandleSubscription(IObserver<ServerEvent> observer, CancellationToken unsubscribeToken)
    {
        Debug.Assert(this.cancellationSource is not null);
        var token = CTSHelper.LinkTokenSource(ref this.cancellationSource, unsubscribeToken);

        this.serverObserver = observer;

        Debug.Assert(LocalEndPoint is not null);
        using TcpListener listener = new(LocalEndPoint);

        try {
            listener.Start();
            CurrentlyListening = true;
            observer.OnNext(new ServerStartupEvent(this));

            do {
                // wait here for new connection
                var newTcpConnection = await listener.AcceptTcpClientAsync(token);
                var connectedClient = CreateClient(newTcpConnection, token);
                this.activeClients[connectedClient.Id] = connectedClient;

                // subscribe to track completion of the client.
                connectedClient.Events.Finally(() => ClientDisconnect(connectedClient))
                               .Subscribe(Stub.DoNothing, Stub.IgnoreError, token);

                // this exposes the client externally so it may be subscribed to.
                observer.OnNext(new ClientConnectionEvent(this, connectedClient));
            } while (true); // only exit is via cancellation.

            // currently no way to reach this endpoint.
        } // operation canceled.
        catch (OperationCanceledException exception) when (exception.CancellationToken.IsCancellationRequested) {
            observer.OnNext(new ServerShutdownEvent(this));
            observer.OnCompleted();
        }
        catch (Exception exception) {
            observer.OnError(exception);
        }
        finally { // server shutdown
            DisposeClients();
            listener.Stop();
            CurrentlyListening = false;

            this.serverObserver = null;
        }
    }

    void ClientDisconnect(IClient client)
    {
        this.serverObserver?.OnNext(new ClientDisconnectEvent(this, client));
        _ = this.activeClients.TryRemove(client.Id, out var _) || ThrowInvalidOperationException<bool>();
        client.Dispose();
    }

    /// <summary>Broadcasts <paramref name="message"/> to <paramref name="recipients"/>.</summary>
    public async Task Broadcast<TMessage>(IEnumerable<TClient> recipients, TMessage message)
        where TMessage : ITransmission
    {
        await Task.WhenAll(recipients.Select(client => client.Transmit(message)));
        this.serverObserver?.OnNext(new ServerBroadcastEvent(this, message));
    }

    void DisposeClients()
    {
        foreach (var client in this.activeClients.Values) client.Dispose();
    }

    public async Task<IDisposable> Stop()
    {
        if (!CurrentlyListening) ThrowInvalidOperationException("Server not started");

        Debug.Assert(this.cancellationSource is not null);
        await this.cancellationSource.CancelAsync();
        
        return this;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        DisposeClients();
        this.cancellationSource?.Cancel();
        this.cancellationSource?.Dispose();
    }
}