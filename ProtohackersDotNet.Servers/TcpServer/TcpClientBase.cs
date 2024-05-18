using System.Reactive.Linq;
using System.Reactive.Subjects;
using static CommunityToolkit.Diagnostics.ThrowHelper;

namespace ProtoHackersDotNet.Servers.TcpServer;

public abstract partial class TcpClientBase<TServer, TClient>(TcpClient client, TServer server, CancellationToken token)
    : IClient, IDisposable
    where TServer : IServer<TClient>
    where TClient : IClient
{
    protected readonly NetworkStream networkStream = client.GetStream();
    protected TServer server = server;
    protected TcpClient client = client;
    protected CancellationToken token = token;

    public Guid Id { get; } = Guid.NewGuid();

    public EndPoint? RemoteEndPoint { get; } = client?.Client.RemoteEndPoint;

    public DateTime ConnectedAt { get; } = DateTime.UtcNow;

    public string? StatusExtended { get; private set; }

    #region Observables

    public IObservable<ConnectionStatus> ConnectionStatusChanges => connectionStatusSubject.AsObservable();
    readonly BehaviorSubject<ConnectionStatus> connectionStatusSubject = new(ConnectionStatus.Connected);
    public ConnectionStatus ConnectionStatus {
        get => this.connectionStatusSubject.Value;
        protected set => this.connectionStatusSubject.OnNext(value);
    }

    public IObservable<string?> StatusChanges => statusSubject.AsObservable();
    readonly BehaviorSubject<string?> statusSubject = new(null);
    protected string? Status {
        get => this.statusSubject.Value;
        set => this.statusSubject.OnNext(value);
    }

    public IObservable<ByteSize> TotalBytesTransmittedChanges => totalbytesTransmittedSubject.AsObservable();
    readonly BehaviorSubject<ByteSize> totalbytesTransmittedSubject = new(ByteSize.FromBytes(0));
    protected ByteSize TotalBytesTransmitted {
        get => this.totalbytesTransmittedSubject.Value;
        set => this.totalbytesTransmittedSubject.OnNext(value);
    }

    public IObservable<ByteSize> TotalBytesRecievedChanges => totalbytesRecievedSubject.AsObservable();
    readonly BehaviorSubject<ByteSize> totalbytesRecievedSubject = new(ByteSize.FromBytes(0));
    protected ByteSize TotalBytesRecieved {
        get => this.totalbytesRecievedSubject.Value;
        set => this.totalbytesRecievedSubject.OnNext(value);
    }

    #endregion

    #region Events

    public event EventHandler<DataTransmission>? DataTransmitted;
    void NotifyDataTransmitted(string? message, ByteSize bytesTransmitted, bool broadcast) => DataTransmitted?.Invoke(this, new() {
        EndPoint = RemoteEndPoint,
        Broadcast = broadcast,
        BytesTransmitted = bytesTransmitted,
        Message = message ?? string.Empty,
    });

    public event EventHandler<DataReciept>? DataRecieved;

    public event EventHandler<Exception>? Exception;

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
            do {
                readResult = await reader.ReadAsync(this.token);

                var bytesRecieved = readResult.Buffer.ToByteSize();
                TotalBytesRecieved += bytesRecieved;

                DataRecieved?.Invoke(this, new() {
                    EndPoint = RemoteEndPoint,
                    BytesRecieved = bytesRecieved,
                    Message = TranslateReciept(readResult.Buffer) ?? string.Empty
                });

                var buffer = readResult.Buffer;
                SequencePosition? lineEnd;
                do {
                    lineEnd = FindLineEnd(buffer);
                    if (lineEnd is null)
                        break;

                    var line = buffer.Slice(0, lineEnd.Value);
                    await ProcessLine(line);

                    buffer = buffer.Slice(lineEnd.Value);
                } while (buffer.Length > 0);

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted && !readResult.Buffer.IsEmpty)
                    ThrowInvalidDataException("Incomplete message.");
            } while (!readResult.IsCompleted);

            ConnectionStatus = ConnectionStatus.Disconnected;
        }
        catch (Exception exception) {
            await OnException(exception);

            // this.client.GetStream().Close();
            // this.client.Close();

            ConnectionStatus = ConnectionStatus.Terminated;
            StatusExtended = exception.Message;

            Exception?.Invoke(this, exception);
        }
        finally {
            await OnDisconnect();
            await reader.CompleteAsync();
            this.client.Close();
        }
    }

    protected abstract string? TranslateReciept(ReadOnlySequence<byte> buffer);

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
        await networkStream.WriteAsync(transmission.Data, this.token);

        var bytesTransmitted = transmission.Data.ToByteSize();
        TotalBytesTransmitted += bytesTransmitted;

        NotifyDataTransmitted(transmission.Translation, bytesTransmitted, transmission.Broadcast);
    }

    public async Task Transmit(ReadOnlyMemory<byte> data, string translation, bool broadcast)
    {
        await networkStream.WriteAsync(data, this.token);

        var bytesTransmitted = data.ToByteSize();
        TotalBytesTransmitted += bytesTransmitted;

        NotifyDataTransmitted(translation, bytesTransmitted, broadcast);
    }

    /// <summary>Transmits <paramref name="data"/> without attempting to translate it for messaging.</summary>
    /// <param name="data">The data to transmit.</param>
    /// <returns>A task that represents completion of the transmission.</returns>
    public async Task Transmit(ReadOnlySequence<byte> data, bool broadcast)
    {
        var position = data.Start;
        while (data.TryGet(ref position, out var memory))
            await networkStream.WriteAsync(memory, this.token);

        var bytesTransmitted = data.ToByteSize();
        TotalBytesTransmitted += bytesTransmitted;

        NotifyDataTransmitted($"{bytesTransmitted} transmitted.", bytesTransmitted, broadcast);
    }

    #endregion

    public virtual void Dispose()
    {
        GC.SuppressFinalize(this);
        this.networkStream.Flush();
        this.networkStream.Dispose();
        this.client.Dispose();
    }
}