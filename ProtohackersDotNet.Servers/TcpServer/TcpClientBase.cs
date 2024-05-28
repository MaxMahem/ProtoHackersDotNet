using System;
using System.IO.Pipelines;
using System.Reactive;
using System.Reactive.Threading.Tasks;
using System.Reflection.PortableExecutable;
using static CommunityToolkit.Diagnostics.ThrowHelper;
using CI = ProtoHackersDotNet.Servers.Interface.Client;

namespace ProtoHackersDotNet.Servers.TcpServer;

public abstract partial class TcpClientBase<TServer> : IClient, IDisposable 
    where TServer : IServer<IClient>
{
    public TcpClientBase(TServer server, TcpClient client, CancellationToken token)
    {
        this.client = client;
        this.cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        Server = server;
        
        this.networkStream = client.GetStream();

        ClientEndPoint = client.Client.RemoteEndPoint as IPEndPoint ?? ThrowArgumentNullException<IPEndPoint>();

        Events = Observable.Create<ClientEvent>(HandleSubscription).Finally(Dispose).Publish().RefCount();
    }

    readonly TcpClient client;
    readonly NetworkStream networkStream;
    CancellationTokenSource cancellationSource;
    /// <summary>External observer of the event stream. Null after the client disconnects.</summary>
    IObserver<ClientEvent>? clientEventObserver;

    public TServer Server { get; }
    IServer IClient.Server => Server;

    public Guid Id { get; } = Guid.NewGuid();

    public IPEndPoint ClientEndPoint { get; }

    public DateTimeOffset ConnectedAt { get; } = DateTimeOffset.UtcNow;

    #region Observables

    public IObservable<ClientEvent> Events { get; }

    readonly BehaviorSubject<ConnectionStatus> connectionStatusObserver = new(CI.ConnectionStatus.Connected);
    public IObservable<ConnectionStatus> ConnectionStatus => connectionStatusObserver.AsObservable();
    public ConnectionStatus LatestConnectionStatus {
        get => this.connectionStatusObserver.Value;
        protected set => this.connectionStatusObserver.OnNext(value);
    }

    readonly BehaviorSubject<string?> statusObserver = new(null);
    public IObservable<string?> Status => statusObserver.AsObservable();
    protected string? LatestStatus {
        get => this.statusObserver.Value;
        set => this.statusObserver.OnNext(value);
    }

    readonly BehaviorSubject<ByteSize> totalBytesTransmittedObserver = new(ByteSize.FromBytes(0));
    public IObservable<ByteSize> TotalBytesTransmitted => totalBytesTransmittedObserver.AsObservable();
    protected ByteSize TotalBytesTransmittedValue {
        get => this.totalBytesTransmittedObserver.Value;
        set => this.totalBytesTransmittedObserver.OnNext(value);
    }

    readonly BehaviorSubject<ByteSize> totalBytesReceivedObserver = new(ByteSize.FromBytes(0));

    public IObservable<ByteSize> TotalBytesReceived => totalBytesReceivedObserver.AsObservable();
    protected ByteSize TotalBytesReceivedValue {
        get => this.totalBytesReceivedObserver.Value;
        set => this.totalBytesReceivedObserver.OnNext(value);
    }

    #endregion

    public async Task HandleSubscription(IObserver<ClientEvent> observer, CancellationToken unsubscribeToken)
    {
        var token = CTSHelper.LinkTokenSource(ref this.cancellationSource, unsubscribeToken);
        var reader = PipeReader.Create(networkStream);
        this.clientEventObserver = observer;

        try {
            await OnConnect(this.cancellationSource.Token);            
            ReadResult readResult;

            do {// hold here for a client transmission
                readResult = await reader.ReadAsync(token);
                var buffer = readResult.Buffer;

                TotalBytesReceivedValue += buffer.ToByteSize();
                Transmission transmission = new(buffer, TranslateReception(buffer), false);
                observer.OnNext(new DataReceptionEvent(this, transmission));

                while (buffer.Length > 0) {
                    SequencePosition? lineEnd = FindLineEnd(buffer); 
                    if (lineEnd is null) break;    // In the case of no line end, break, giving back the unsliced buffer.

                    (var line, buffer) = buffer.Divide(lineEnd.Value); // split the line out from the remaining buffer.
                    await ProcessLine(line);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted && !readResult.Buffer.IsEmpty) IncompleteMessageException.Throw(this);
            } while (!readResult.IsCompleted);

            LatestConnectionStatus = CI.ConnectionStatus.Disconnected;
            observer.OnCompleted();
        }
        // Error resulting from bad client input. Report and terminate.
        catch (ClientException exception) {
            await OnException(exception, token);

            LatestConnectionStatus = CI.ConnectionStatus.Terminated;
            observer.OnError(exception);
        }
        catch (Exception exception) {
            LatestConnectionStatus = CI.ConnectionStatus.Error;
            observer.OnError(exception);
        }
        finally {
            await OnDisconnect(token);
            await reader.CompleteAsync();
            Dispose();
        }
    }

    #region Abstract overloads - Clients need to implement these.

    /// <summary>When overridden by a child, processes a single line of data from the client.</summary>
    /// <param name="line">The line of data to process.</param>
    /// <returns>A task that represents completion of processing of this line.</returns>
    protected abstract Task ProcessLine(ReadOnlySequence<byte> line);

    protected abstract string? TranslateReception(ReadOnlySequence<byte> buffer);

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
    public async Task Transmit(ITransmission transmission)
    {
        await networkStream.WriteAsync(transmission.Data, this.cancellationSource.Token);

        var bytesTransmitted = transmission.Data.ToByteSize();
        TotalBytesTransmittedValue += bytesTransmitted;

        this.clientEventObserver?.OnNext(DataTransmissionEvent.FromTransmission(this, transmission));
    }

    /// <summary>Transmits <paramref name="data"/> without attempting to translate it for messaging.</summary>
    /// <param name="data">The data to transmit.</param>
    /// <returns>A task that represents completion of the transmission.</returns>
    public async Task Transmit(ReadOnlySequence<byte> data, bool broadcast)
    {
        var position = data.Start;
        while (data.TryGet(ref position, out var memory))
            await networkStream.WriteAsync(memory, this.cancellationSource.Token);

        var bytesTransmitted = data.ToByteSize();
        TotalBytesTransmittedValue += bytesTransmitted;

        Transmission transmission = new(data, $"{bytesTransmitted} transmitted", broadcast);
        this.clientEventObserver?.OnNext(DataTransmissionEvent.FromTransmission(this, transmission));
    }

    #endregion

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        this.clientEventObserver = null;
        this.networkStream.Flush();
        this.networkStream.Dispose();
        this.cancellationSource.Dispose();
        this.client.Dispose();
    }
}