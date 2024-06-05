using ProtoHackersDotNet.Helpers.ObservableTypes;
using System.Collections.Concurrent;
using System.IO.Hashing;
using IServerStatus = ProtoHackersDotNet.Servers.Interface.Server.ServerStatus;
using ReadOnlyBytes = System.ReadOnlyMemory<byte>;

namespace ProtoHackersDotNet.Servers.UdpDatabase;

public class UdpDatabaseServer(UdpDatabaseServerOptions options) : IServer<IClient>
{
    const byte KEY_SEPERATOR = (byte)'=';
    public const int MAX_MESSAGE_BYTES = 1000;

    CancellationTokenSource cancellationSource = new();

    readonly ConcurrentDictionary<ReadOnlyBytes, ReadOnlyBytes> dictionary
        = new(ReadOnlyBytesComparer.Default);

    public ServerName Name => ServerName.From(nameof(UdpDatabaseServer));

    public Problem Solution => new(4, "UdpDatabase");

    public IPEndPoint? LocalEndPoint { get; private set; }

    readonly ObservableValue<IServerStatus> serverStatusObservable = new(IServerStatus.Stopped);
    public IObservable<IServerStatus> ServerStatus => this.serverStatusObservable.Values;
    public IObservable<bool> Listening => ServerStatus.Select(status => status is IServerStatus.Listening)
                                                      .DistinctUntilChanged();

    public virtual IObservable<string?> Status => Observable.Return<string?>(null);

    public IEnumerable<IClient> Clients => [];

    public IConnectableObservable<IEvent> Start(IPEndPoint endPoint, CancellationToken token = default)
    {
        if (this.serverStatusObservable.LatestValue is IServerStatus.Listening)
            ThrowInvalidOperationException("Server already started");
        this.cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        LocalEndPoint = endPoint;

        return Observable.Create<IEvent>(HandleSubscription).Publish();
    }

    async Task HandleSubscription(IObserver<IEvent> observer, CancellationToken unsubscribeToken)
    {
        Debug.Assert(this.cancellationSource is not null);
        var token = CTSHelper.LinkTokenSource(ref this.cancellationSource, unsubscribeToken);

        Debug.Assert(LocalEndPoint is not null);
        UdpClient udpClient = new(LocalEndPoint);

        try {
            this.serverStatusObservable.LatestValue = IServerStatus.Listening;

            observer.OnNext(new ServerStartupEvent(this));

            do {// wait here for new connection
                var receipt = await udpClient.ReceiveAsync(token);

                await ProcessReceipt(receipt, udpClient, observer, token);
            } while (true); // only exit is via exception.
        } // operation canceled.
        catch (OperationCanceledException exception) when (exception.CancellationToken.IsCancellationRequested) {
            observer.OnNext(new ServerShutdownEvent(this));
            observer.OnCompleted();
            this.serverStatusObservable.LatestValue = IServerStatus.Stopped;
        } // unhandled exception.
        catch (Exception exception) {
            observer.OnNext(new ServerTerminatedEvent(this, exception));
            observer.OnError(exception);
            this.serverStatusObservable.LatestValue = IServerStatus.Terminated;
        }
        finally { // server shutdown
            udpClient.Close();
        }
    }

    async Task ProcessReceipt(UdpReceiveResult receipt, UdpClient udpClient, IObserver<IEvent> observer, 
        CancellationToken token)
    {
        if (receipt.Buffer.Length > MAX_MESSAGE_BYTES)
            ThrowInvalidDataException($"Response length ({receipt.Buffer.Length}) exceeds maximum ({MAX_MESSAGE_BYTES})");

        var receiptTranslation = Encoding.ASCII.GetString(receipt.Buffer);
        observer.OnNext(DataReceptionEvent.FromServer(this, receiptTranslation, receipt.RemoteEndPoint));

        switch (receipt.Buffer.AsSpan().IndexOf(KEY_SEPERATOR)) {
            case < 0: {  // no key found. Retrieve.
                var data = RetrieveData(receipt.Buffer);
                if (data is null) break; // key not found

                if (receipt.Buffer.Length + data.Value.Length > MAX_MESSAGE_BYTES)
                    ThrowInvalidDataException("Response length exceeds maximum.");
                byte[] response = [..receipt.Buffer, ..data.Value.Span];

                int bytesSent = await udpClient.SendAsync(response, receipt.RemoteEndPoint, token);
                Debug.Assert(bytesSent == response.Length);

                var transmission = UdpTransmission.FromData(response);
                observer.OnNext(DataTransmissionEvent.FromServer(this, transmission));
            }
            break;
            case int index when index >= 0:  // key found. Insert.
                InsertData(receipt.Buffer.AsSpan()[..index],
                           receipt.Buffer.AsSpan()[index..]);
                break;
        }
    }

    readonly static ReadOnlyBytes VersionKey = Encoding.ASCII.GetBytes("version");
    readonly ReadOnlyBytes VersionValue = Encoding.Default.GetBytes(options.Version);

    ReadOnlyBytes? RetrieveData(byte[] buffer) => buffer switch {
        _ when buffer.AsSpan().SequenceEqual(VersionKey.Span) => VersionValue,
        _ when this.dictionary.TryGetValue(buffer.AsMemory(), out var dbValue) => dbValue,
        _ => null
    };

    /// <summary>Insert the given data into the dictionary, overwriting previous entries if present.</summary>
    /// <remarks>Since all responses would need the '=' prefixed anyways, <paramref name="value"/>s are
    /// sliced and saved with the '=' still present.</remarks>
    void InsertData(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        if (key.SequenceEqual(VersionKey.Span)) return;
        this.dictionary[key.ToArray()] = value.ToArray();
    }

    public async Task<IDisposable> Stop()
    {
        if (this.serverStatusObservable.LatestValue is not IServerStatus.Listening)
            ThrowInvalidOperationException("Server not started");

        Debug.Assert(this.cancellationSource is not null);
        await this.cancellationSource.CancelAsync();

        return this;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        this.cancellationSource?.Cancel();
        this.cancellationSource?.Dispose();
        this.serverStatusObservable.Dispose();
    }

    public class ReadOnlyBytesComparer : IEqualityComparer<ReadOnlyBytes>
    {
        public bool Equals(ReadOnlyBytes x, ReadOnlyBytes y) => x.Span.SequenceEqual(y.Span);

        public int GetHashCode(ReadOnlyBytes data)
            => unchecked((int) XxHash32.HashToUInt32(data.Span));

        public static readonly ReadOnlyBytesComparer Default = new();
    }
}