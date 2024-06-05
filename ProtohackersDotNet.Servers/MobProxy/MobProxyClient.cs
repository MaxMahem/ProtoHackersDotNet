using ProtoHackersDotNet.Helpers.ObservableTypes;
using ProtoHackersDotNet.Servers.Interface.Client;
using System.Net.Sockets;
using System.Reactive.Disposables;
using IConnectionStatus = ProtoHackersDotNet.Servers.Interface.Client.ConnectionStatus;

namespace ProtoHackersDotNet.Servers.MobProxy;

public sealed class MobProxyClient : IClient
{
    const byte LINE_DELIMITER = (byte) '\n';
    const byte WORD_DELIMITER = (byte) ' ';
    const int MIN_ADDRESS_LENGTH = 26;
    const int MAX_ADDRESS_LENGTH = 35;
    const byte ADDRESS_START_BYTE = (byte) '7';
    readonly static ReadOnlyMemory<byte> LineDelimiterMemory = new([LINE_DELIMITER]);
    readonly static ReadOnlyMemory<byte> WordDelimiterMemory = new([WORD_DELIMITER]);

    /// <summary>Creates a <see cref="MobProxyClient"/> for data from the remote client, to the proxy ip. Created by server.</summary>
    /// <param name="options">Options to use with this client.</param>
    /// <param name="downstreamClient">The tcp client this client should use to communicate. Connects from the remote client.</param>
    /// <param name="token">A cancellation token used to cancel client operations.</param>
    public MobProxyClient(MobProxyClientOptions options, TcpClient downstreamClient, CancellationToken token)
    {
        this.options = options;
        this.cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(token);

        TcpClient upstreamClient = new(options.ChatServer.Host, options.ChatServer.Port);
        this.networkUpstream = upstreamClient.GetStream();
        this.networkDownstream = downstreamClient.GetStream();

        LocalEndPoint = (IPEndPoint) downstreamClient.Client.LocalEndPoint!;
        ClientEndPoint = (IPEndPoint) downstreamClient.Client.RemoteEndPoint!;

        Events = Observable.Merge(
            Observable.Create<IEvent>((observer, token) => BuildConnectionHandler(this.networkUpstream, this.networkDownstream, observer, token)),
            Observable.Create<IEvent>((observer, token) => BuildConnectionHandler(this.networkDownstream, this.networkUpstream, observer, token)),
            this.transmissionObserver
        ).Finally(DisposeConnection).Publish().AutoConnect(3);

        this.connectionDisposables = [upstreamClient, downstreamClient, this.networkUpstream, this.networkDownstream, this.cancellationSource];
        this.observableDisposables = [
            this.connectionStatusObservable,
            this.totalBytesReceivedObservable,
            this.totalBytesTransmittedObservable,
            this.transmissionObserver
        ];

        Task BuildConnectionHandler(NetworkStream inStream, NetworkStream outStream, IObserver<IEvent> observer, CancellationToken token)
            => HandleConnection(inStream, sequence => Transmit(outStream, sequence), observer, token);
    }

    readonly MobProxyClientOptions options;
    readonly Subject<IEvent> transmissionObserver = new();
    readonly CompositeDisposable connectionDisposables, observableDisposables;
    readonly NetworkStream networkUpstream, networkDownstream;
    CancellationTokenSource cancellationSource;

    public IObservable<IEvent> Events { get; }

    public IPEndPoint LocalEndPoint { get; }

    public IPEndPoint ClientEndPoint { get; }

    #region Observables

    readonly Subject<IEvent> eventsObservable = new();

    readonly ObservableValue<IConnectionStatus> connectionStatusObservable = new(IConnectionStatus.Connected);
    public IObservable<IConnectionStatus> ConnectionStatus => this.connectionStatusObservable.Values;
    public IConnectionStatus LatestConnectionStatus => this.connectionStatusObservable.LatestValue;

    public IObservable<string?> Status => Observable.Return<string?>(null);

    readonly ObservableValue<ByteSize> totalBytesTransmittedObservable = new(ByteSize.FromBytes(0));
    public IObservable<ByteSize> TotalBytesTransmitted => this.totalBytesTransmittedObservable.Values;

    readonly ObservableValue<ByteSize> totalBytesReceivedObservable = new(ByteSize.FromBytes(0));
    public IObservable<ByteSize> TotalBytesReceived => this.totalBytesReceivedObservable.Values;

    #endregion

    /// <summary>Handle processing of data from from the connection and providing that information to subscribers.</summary>
    /// <param name="observer">The observer to notify of connection events.</param>
    /// <param name="unsubscribeToken">A token that can be used to trigger unsubscription.</param>
    /// <returns>A task that indicates completion of all events.</returns>
    async Task HandleConnection(NetworkStream networkStream, Func<ReadOnlySequence<byte>, Task> transmitter, 
        IObserver<IEvent> observer, CancellationToken unsubscribeToken)
    {
        var token = CTSHelper.LinkTokenSource(ref this.cancellationSource, unsubscribeToken);
        var reader = PipeReader.Create(networkStream);

        try {
            ReadResult readResult;

            do {// hold here for a client transmission
                readResult = await reader.ReadAsync(token);

                var buffer = readResult.Buffer;

                this.totalBytesReceivedObservable.LatestValue += buffer.ToByteSize();
                observer.OnNext(DataReceptionEvent.FromClient(this, TranslateReception(buffer)));

                while (buffer.Length > 0) {
                    SequencePosition? lineEnd = buffer.PositionOf(LINE_DELIMITER, 1);
                    if (lineEnd is null) break;    // In the case of no line end, break, giving back the unsliced buffer.

                    // split the line out from the remaining buffer.
                    (var line, buffer) = buffer.Divide(lineEnd.Value); 

                    line = ReplaceAddresses(line);
                    await transmitter(line);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted && !readResult.Buffer.IsEmpty)
                    IncompleteMessageException.Throw(this);
            } while (!readResult.IsCompleted);

            this.connectionStatusObservable.LatestValue = IConnectionStatus.Disconnected;
            this.transmissionObserver.OnCompleted();
            observer.OnCompleted();
        }
        // Error resulting from bad client input. Report and terminate.
        catch (ClientException exception) {

            this.connectionStatusObservable.LatestValue = IConnectionStatus.Exception;
            observer.OnError(exception);
        }
        // Force close from client end.
        catch (IOException exception) {

            this.connectionStatusObservable.LatestValue = IConnectionStatus.Exception;
            observer.OnError(exception);
        }
        catch (Exception exception) {
            this.connectionStatusObservable.LatestValue = IConnectionStatus.Exception;
            observer.OnError(exception);
        }
        finally {
            await reader.CompleteAsync();
            DisposeConnection();
        }
    }

    ReadOnlySequence<byte> ReplaceAddresses(ReadOnlySequence<byte> line)
    {
        AppendableSequence<byte> workingSequence = new();
        SequencePosition position = line.Start;
        SequencePosition? delimiterPosition;

        while ((delimiterPosition = line.PositionOf(WORD_DELIMITER)) is not null) {
            // Slice up to the delimiter (exclusive)
            var segment = line.Slice(position, delimiterPosition.Value);

            if (IsAddress(segment)) workingSequence.Append(this.options.ReplacementAddress);
            else workingSequence.Append(segment);

            workingSequence.Append(WordDelimiterMemory);

            // advance past the delimiter
            position = line.GetPosition(1, delimiterPosition.Value); 
            line = line.Slice(position);
        }

        // Handle the trailing segment after the last delimiter
        if (!line.IsEmpty)
            if (IsAddress(line)) {
                workingSequence.Append(this.options.ReplacementAddress);
                workingSequence.Append(LineDelimiterMemory);
            }
            else workingSequence.Append(line);

        return workingSequence;
    }

    /// <summary>Transmits <paramref name="data"/> without attempting to translate it for messaging.</summary>
    /// <param name="data">The data to transmit.</param>
    /// <returns>A task that represents completion of the transmission.</returns>
    async Task Transmit(NetworkStream networkStream, ReadOnlySequence<byte> data)
    {
        var position = data.Start;
        while (data.TryGet(ref position, out var memory))
            await networkStream.WriteAsync(memory, this.cancellationSource.Token);

        var bytesTransmitted = data.ToByteSize();
        this.totalBytesTransmittedObservable.LatestValue += bytesTransmitted;

        Transmission transmission = new(){
            Data = data.ToArray(),
            Translation = Encoding.ASCII.GetString(data),
        };
        this.transmissionObserver.OnNext(DataTransmissionEvent.FromClient(this, transmission));
    }

    static bool IsAddress(ReadOnlySequence<byte> segment)
        => segment.Length >= MIN_ADDRESS_LENGTH && segment.Length <= MAX_ADDRESS_LENGTH
            && segment.FirstSpan[0] == ADDRESS_START_BYTE && segment.IsAlphanumeric();

    static string TranslateReception(ReadOnlySequence<byte> buffer)
        => Encoding.ASCII.GetString(buffer);

    void DisposeConnection() => this.connectionDisposables.Dispose();

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        Debug.Assert(this.connectionDisposables.IsDisposed);
        this.observableDisposables.Dispose();
    }
}

public static class AsciiSequenceHelper
{
    public static bool IsAlphanumeric(this ReadOnlySequence<byte> sequence)
        => sequence.PositionOfAnyExceptInRange<byte>(0x00, 0x2F) is not null  // Control characters and punctuation
		&& sequence.PositionOfAnyExceptInRange<byte>(0x3A, 0x40) is not null  // : ; < = > ? @
        && sequence.PositionOfAnyExceptInRange<byte>(0x5B, 0x60) is not null  // [ \ ] ^ _ `
        && sequence.PositionOfAnyExceptInRange<byte>(0x7B, 0xFF) is not null; // { | } ~ and beyond
}