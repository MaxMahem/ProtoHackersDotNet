using ProtoHackersDotNet.Servers.Interfaces.Client.Messages;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using static CommunityToolkit.Diagnostics.ThrowHelper;
using CI = ProtoHackersDotNet.Servers.Interfaces.Client;

namespace ProtoHackersDotNet.Servers.TcpServer;

public abstract partial class TcpClientBase<TServer>(TServer server, TcpClient client, CancellationToken token)
    : IClient, IDisposable 
    where TServer : IServer<IClient>
{
    readonly NetworkStream networkStream = client.GetStream();
    protected TcpClient Client { get; } = client;
    protected CancellationToken Token { get; } = token;
    public TServer Server { get; } = server;
    IServer IClient.Server => Server;

    public Guid Id { get; } = Guid.NewGuid();

    public EndPoint EndPoint { get; } = client.Client.RemoteEndPoint ?? ThrowArgumentNullException<EndPoint>();

    public DateTime ConnectedAt { get; } = DateTime.UtcNow;

    #region Observables

    readonly BehaviorSubject<ConnectionStatus> connectionStatusObserver = new(CI.ConnectionStatus.Connected);
    public IObservable<ConnectionStatus> ConnectionStatus => connectionStatusObserver.AsObservable();
    public ConnectionStatus CurrentConnectionStatus {
        get => this.connectionStatusObserver.Value;
        protected set => this.connectionStatusObserver.OnNext(value);
    }

    readonly BehaviorSubject<string?> statusObserver = new(null);
    public IObservable<string?> Status => statusObserver.AsObservable();
    protected string? StatusValue {
        get => this.statusObserver.Value;
        set => this.statusObserver.OnNext(value);
    }

    readonly BehaviorSubject<ByteSize> totalbytesTransmittedObserver = new(ByteSize.FromBytes(0));
    public IObservable<ByteSize> TotalBytesTransmitted => totalbytesTransmittedObserver.AsObservable();
    protected ByteSize TotalBytesTransmittedValue {
        get => this.totalbytesTransmittedObserver.Value;
        set => this.totalbytesTransmittedObserver.OnNext(value);
    }

    readonly BehaviorSubject<ByteSize> totalbytesRecievedObserver = new(ByteSize.FromBytes(0));
    public IObservable<ByteSize> TotalBytesRecieved => totalbytesRecievedObserver.AsObservable();
    protected ByteSize TotalBytesRecievedValue {
        get => this.totalbytesRecievedObserver.Value;
        set => this.totalbytesRecievedObserver.OnNext(value);
    }

    readonly Subject<ITransmission> transmissionObserver = new();
    public IObservable<ITransmission> Transmissions => transmissionObserver.AsObservable();
    void NotifyDataTransmission(ITransmission transmission) => this.transmissionObserver.OnNext(transmission);

    readonly Subject<ITransmission> receiptObserver = new();
    public IObservable<ITransmission> Receptions => this.receiptObserver.AsObservable();
    void NotifyDataRecieved(ReadOnlySequence<byte> recievedData)
    {
        var message = TranslateRecieption(recievedData) ?? $"{recievedData.ToByteSize()} recieved.";
        this.receiptObserver.OnNext(new Reception(recievedData, message));
    }

    readonly Subject<Exception> exceptionSubject = new();
    public IObservable<Exception> Exceptions => this.exceptionSubject.AsObservable();

    void NotifyClientException(Exception exception) => this.exceptionSubject.OnNext(exception);

    #endregion

    /// <summary>When overriden by a child, processes a single line of data from the client.</summary>
    /// <param name="line">The line of data to process.</param>
    /// <returns>A task that represents completion of processing of this line.</returns>
    protected abstract Task ProcessLine(ReadOnlySequence<byte> line);

    public async Task HandleClient()
    {
        await OnConnect();

        var reader = PipeReader.Create(networkStream);
        ReadResult readResult;

        try {
            do {// hold here for a client transmission, then pull in the entire thing
                readResult = await reader.ReadAsync(Token);
                var buffer = readResult.Buffer;

                TotalBytesRecievedValue += buffer.ToByteSize();
                NotifyDataRecieved(buffer);

                SequencePosition? lineEnd;
                while (buffer.Length > 0) {        // divide the buffer up into lines that can be processed.
                    lineEnd = FindLineEnd(buffer); // if this returns null, no line end was found.
                    if (lineEnd is null) break;    // Break and, give back the unused buffer (since it has not been sliced)

                    // process the identified chunk of line.
                    var line = buffer.Slice(0, lineEnd.Value);
                    await ProcessLine(line);

                    // slice beyond the consumed line.
                    buffer = buffer.Slice(lineEnd.Value);
                }

                // advanced the buffer forward. The previous slice puts start at the last consumed position.
                // End will always mark the end of what has been "seen" by the buffer.
                reader.AdvanceTo(buffer.Start, buffer.End);

                // looping with a partially consumed buffer is fine, but if the client disconnets while
                // there is still data in the buffer, we recieved an incomplete message.
                if (readResult.IsCompleted && !readResult.Buffer.IsEmpty)
                    ThrowInvalidDataException("Incomplete message.");
            } while (!readResult.IsCompleted);

            CurrentConnectionStatus = CI.ConnectionStatus.Disconnected;
        }
        catch (Exception exception) {
            await OnException(exception);

            CurrentConnectionStatus = CI.ConnectionStatus.Terminated;

            NotifyClientException(exception);
        }
        finally {
            await OnDisconnect();
            await reader.CompleteAsync();
            this.Client.Close();
            this.connectionStatusObserver.OnCompleted();
            Dispose();
        }
    }

    protected abstract string? TranslateRecieption(ReadOnlySequence<byte> buffer);

    /// <summary>When overridden by a child, performs class specific behavior when a client connects.
    /// Base class does nothing.</summary>
    /// <returns>A task that represents completion of the OnConnect behavior.</returns>
    protected virtual Task OnConnect() => Task.CompletedTask;

    /// <summary>When overridden by a child, performs class specific behavior when a client disconnects.
    /// Base class does nothing.</summary>
    /// <returns>A task that represents completion of the OnDisconnect behavior.</returns>
    protected virtual Task OnDisconnect() => Task.CompletedTask;

    /// <summary>When overridden by a child, performs class specific behavior when an exception occurs.
    /// Base class does nothing.</summary>
    /// <param name="exception">The exception that was thrown.</param>
    /// <returns>A task that represents completion of the OnException behavior.</returns>
    protected virtual Task OnException(Exception exception) => Task.CompletedTask;

    /// <summary>When overriden by a child, finds the position of the end of a line of data within 
    /// <paramref name="buffer"/>, or <c>null</c> if a line end could not be found.</summary>
    /// <param name="buffer">The buffer to search for a line ending in.</param>
    /// <returns>The position of the line ending, or <c>null</c> if no line end could be found.</returns>
    protected abstract SequencePosition? FindLineEnd(ReadOnlySequence<byte> buffer);

    #region Transmit

    /// <summary>Transmits data to the connected client, and notifies subscribers of the transmission.</summary>
    /// <param name="transmission">Object containing data and a translation for transmission.</param>
    /// <returns>A task that represents completion of this transmission.</returns>
    public async Task Transmit(ITransmission transmission)
    {
        await networkStream.WriteAsync(transmission.Data, this.Token);

        var bytesTransmitted = transmission.Data.ToByteSize();
        TotalBytesTransmittedValue += bytesTransmitted;

        NotifyDataTransmission(transmission);
    }

    /// <summary>Transmits <paramref name="data"/> without attempting to translate it for messaging.</summary>
    /// <param name="data">The data to transmit.</param>
    /// <returns>A task that represents completion of the transmission.</returns>
    public async Task Transmit(ReadOnlySequence<byte> data, bool broadcast, string? message = null)
    {
        var position = data.Start;
        while (data.TryGet(ref position, out var memory))
            await networkStream.WriteAsync(memory, this.Token);

        var bytesTransmitted = data.ToByteSize();
        TotalBytesTransmittedValue += bytesTransmitted;

        Transmission transmission = new(data, $"{bytesTransmitted} transmitted.", broadcast);
        NotifyDataTransmission(transmission);
    }

    #endregion

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        this.networkStream.Flush();
        this.networkStream.Dispose();
        this.Client.Dispose();
    }
}