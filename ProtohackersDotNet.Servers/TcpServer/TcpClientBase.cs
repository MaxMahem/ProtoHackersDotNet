using System.Reactive.Disposables;
using ProtoHackersDotNet.Helpers.ObservableTypes;
using IConnectionStatus = ProtoHackersDotNet.Servers.Interface.Client.ConnectionStatus;

namespace ProtoHackersDotNet.Servers.TcpServer;

public abstract class TcpClientBase : IClient, IDisposable 
{
    /// <summary>Creates a new instance of this class, setting its private properties.</summary>
    /// <remarks>Also starts the <see cref="TcpClientBase{TServer}.Events"/> internal subscriptions.
    /// Will be published when the <b>first</b> external observer subscribes.</remarks>
    /// <param name="client">The tcp client this client should use to communicate.</param>
    /// <param name="token">A cancellation token used to cancel client operations.</param>
    public TcpClientBase(TcpClient client, CancellationToken token)
    {
        this.cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        this.networkStream = client.GetStream();

        ClientEndPoint = (IPEndPoint) client.Client.RemoteEndPoint!;
        LocalEndPoint = (IPEndPoint) client.Client.LocalEndPoint!;

        Events = Observable.Create<IEvent>(HandleEventSubscription).Finally(DisposeConnection)
            .Merge(transmissionObserver).Publish().AutoConnect(3);

        this.connectionDisposables = [ client, this.cancellationSource, this.networkStream ];
        this.observableDisposables = [ 
            this.connectionStatusObservable, 
            this.totalBytesReceivedObservable, 
            this.totalBytesTransmittedObservable,
            this.transmissionObserver 
        ];
    }

    readonly Subject<IEvent> transmissionObserver = new();
    readonly CompositeDisposable connectionDisposables, observableDisposables;
    readonly NetworkStream networkStream;
    CancellationTokenSource cancellationSource;

    public IObservable<IEvent> Events { get; }

    public IPEndPoint LocalEndPoint { get; }

    public IPEndPoint ClientEndPoint { get; }

    #region Observables

    readonly Subject<IEvent> eventsObservable = new();

    readonly ObservableValue<IConnectionStatus> connectionStatusObservable = new(IConnectionStatus.Connected);
    public IObservable<IConnectionStatus> ConnectionStatus => this.connectionStatusObservable.Values;
    public IConnectionStatus LatestConnectionStatus => this.connectionStatusObservable.LatestValue;

    public virtual IObservable<string?> Status => Observable.Return<string?>(null);

    readonly ObservableValue<ByteSize> totalBytesTransmittedObservable = new(ByteSize.FromBytes(0));
    public IObservable<ByteSize> TotalBytesTransmitted => this.totalBytesTransmittedObservable.Values;

    readonly ObservableValue<ByteSize> totalBytesReceivedObservable = new(ByteSize.FromBytes(0));
    public IObservable<ByteSize> TotalBytesReceived => this.totalBytesReceivedObservable.Values;

    #endregion

    /// <summary>Handle processing of data from from the connection and providing that information to subscribers.</summary>
    /// <param name="observer">The observer to notify of connection events.</param>
    /// <param name="unsubscribeToken">A token that can be used to trigger unsubscription.</param>
    /// <returns>A task that indicates completion of all events.</returns>
    async Task HandleEventSubscription(IObserver<IEvent> observer, CancellationToken unsubscribeToken)
    {
        var token = CTSHelper.LinkTokenSource(ref this.cancellationSource, unsubscribeToken);
        var reader = PipeReader.Create(networkStream);

        try {
            await OnConnect(this.cancellationSource.Token);
            ReadResult readResult;

            do {// hold here for a client transmission
                readResult = await reader.ReadAsync(token);
                
                var buffer = readResult.Buffer;

                this.totalBytesReceivedObservable.LatestValue += buffer.ToByteSize();
                observer.OnNext(DataReceptionEvent.FromClient(this, TranslateReception(buffer)));

                while (buffer.Length > 0) {
                    SequencePosition? lineEnd = FindLineEnd(buffer); 
                    if (lineEnd is null) break;    // In the case of no line end, break, giving back the unsliced buffer.

                    (var line, buffer) = buffer.Divide(lineEnd.Value); // split the line out from the remaining buffer.
                    await ProcessLine(line, token);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted && !readResult.Buffer.IsEmpty) IncompleteMessageException.Throw(this);
            } while (!readResult.IsCompleted);

            this.connectionStatusObservable.LatestValue = IConnectionStatus.Disconnected;
            await OnDisconnect(token);
            this.transmissionObserver.OnCompleted();
            observer.OnCompleted();
        }
        // Error resulting from bad client input. Report and terminate.
        catch (ClientException exception) {
            await OnException(exception, token);

            this.connectionStatusObservable.LatestValue = IConnectionStatus.Exception;
            await OnDisconnect(token);
            observer.OnError(exception);
        }
        // Force close from client end.
        catch (IOException exception) {
            await OnException(exception, token);

            this.connectionStatusObservable.LatestValue = IConnectionStatus.Exception;
            await OnDisconnect(token);
            observer.OnError(exception);
        }
        catch (Exception exception) {
            this.connectionStatusObservable.LatestValue = IConnectionStatus.Exception;
            await OnDisconnect(token);
            observer.OnError(exception);
        }
        finally {
            await reader.CompleteAsync();
            DisposeConnection();
        }
    }

    #region Abstract overloads - Clients need to implement these.

    /// <summary>When overridden by a child, processes a single line of data from the client.</summary>
    /// <param name="line">The line of data to process.</param>
    /// <returns>A task that represents completion of processing of this line.</returns>
    protected abstract Task ProcessLine(ReadOnlySequence<byte> line, CancellationToken token);

    protected abstract string TranslateReception(ReadOnlySequence<byte> buffer);

    /// <summary>When overridden by a child, finds the position of the end of a line of data within 
    /// <paramref name="buffer"/>, or <c>null</c> if a line end could not be found.</summary>
    /// <param name="buffer">The buffer to search for a line ending in.</param>
    /// <returns>The position of the line ending, or <c>null</c> if no line end could be found.</returns>
    protected abstract SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer);

    #endregion

    #region On method for optional client overload. These methods do nothing by default.

    /// <summary>When overridden by a child, performs class specific behavior when a client connects.
    /// Base class does nothing.</summary>
    /// <returns>A task that represents completion of the OnConnect behavior.</returns>
    protected virtual Task OnConnect(CancellationToken token) => Task.CompletedTask;

    /// <summary>When overridden by a child, performs class specific behavior when a client disconnects.
    /// Base class does nothing.</summary>
    /// <returns>A task that represents completion of the OnDisconnect behavior.</returns>
    protected virtual Task OnDisconnect(CancellationToken token) => Task.CompletedTask;

    /// <summary>When overridden by a child, performs class specific behavior when an exception occurs.
    /// Base class does nothing.</summary>
    /// <param name="exception">The exception that was thrown.</param>
    /// <returns>A task that represents completion of the OnException behavior.</returns>
    protected virtual Task OnException(Exception exception, CancellationToken token) => Task.CompletedTask;

    #endregion

    #region Transmit

    /// <summary>Transmits data to the connected client, and notifies subscribers of the transmission.</summary>
    /// <param name="transmission">Object containing data and a translation for transmission.</param>
    /// <returns>A task that represents completion of this transmission.</returns>
    public async Task Transmit(ITransmission transmission, CancellationToken token = default)
    {
        token = CancellationTokenSource.CreateLinkedTokenSource(token, this.cancellationSource.Token).Token;
        await this.networkStream.WriteAsync(transmission.Data, token);

        var bytesTransmitted = transmission.Data.ToByteSize();
        this.totalBytesTransmittedObservable.LatestValue += bytesTransmitted;

        this.transmissionObserver.OnNext(DataTransmissionEvent.FromClient(this, transmission));
    }

    /// <summary>Transmits <paramref name="data"/> without attempting to translate it for messaging.</summary>
    /// <param name="data">The data to transmit.</param>
    /// <returns>A task that represents completion of the transmission.</returns>
    public async Task Transmit(ReadOnlySequence<byte> data, CancellationToken token)
    {
        token = CancellationTokenSource.CreateLinkedTokenSource(token, this.cancellationSource.Token).Token;

        var position = data.Start;
        while (data.TryGet(ref position, out var memory))
            await this.networkStream.WriteAsync(memory, token);

        var bytesTransmitted = data.ToByteSize();
        this.totalBytesTransmittedObservable.LatestValue += bytesTransmitted;

        Transmission transmission = new(){
            Data = data.ToArray(),
            Translation = $"{bytesTransmitted} transmitted",
        };
        this.transmissionObserver.OnNext(DataTransmissionEvent.FromClient(this, transmission));
    }

    #endregion

    void DisposeConnection() => this.connectionDisposables.Dispose();

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);

        Debug.Assert(this.connectionDisposables.IsDisposed);
        this.observableDisposables.Dispose();
    }
}