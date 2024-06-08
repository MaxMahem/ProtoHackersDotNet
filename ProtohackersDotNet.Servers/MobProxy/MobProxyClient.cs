using System.Reactive.Disposables;
using ProtoHackersDotNet.AsciiString;
using ProtoHackersDotNet.Helpers.ObservableTypes;
using IConnectionStatus = ProtoHackersDotNet.Servers.Interface.Client.ConnectionStatus;

namespace ProtoHackersDotNet.Servers.MobProxy;

public sealed class MobProxyClient : IClient
{
    const byte LINE_DELIMITER = (byte) '\n';
    const byte WORD_DELIMITER = (byte) ' ';
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

    enum ConnectionDirection { Upstream, Downstream }

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
    public IObservable<IConnectionStatus> ConnectionStatus => this.connectionStatusObservable.Value;
    public IConnectionStatus LatestConnectionStatus => this.connectionStatusObservable.CurrentValue;

    public IObservable<string?> Status => Observable.Return<string?>(null);

    readonly ObservableValue<ByteSize> totalBytesTransmittedObservable = new(ByteSize.FromBytes(0));
    public IObservable<ByteSize> TotalBytesTransmitted => this.totalBytesTransmittedObservable.Value;

    readonly ObservableValue<ByteSize> totalBytesReceivedObservable = new(ByteSize.FromBytes(0));
    public IObservable<ByteSize> TotalBytesReceived => this.totalBytesReceivedObservable.Value;

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

                this.totalBytesReceivedObservable.CurrentValue += buffer.ToByteSize();
                observer.OnNext(DataReceptionEvent.FromClient(this, TranslateReception(buffer)));

                while (buffer.Length > 0) {
                    SequencePosition? lineEnd = buffer.PositionOf(LINE_DELIMITER, 1);
                    if (lineEnd is null) break;    // In the case of no line end, break, giving back the unsliced buffer.

                    // split the line out from the remaining buffer.
                    (var line, buffer) = buffer.Split(lineEnd.Value); 

                    line = ReplaceAddresses(line);
                    await transmitter(line);
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (readResult.IsCompleted && !readResult.Buffer.IsEmpty)
                    IncompleteMessageException.Throw(this);
            } while (!readResult.IsCompleted);

            this.connectionStatusObservable.CurrentValue = IConnectionStatus.Disconnected;
            this.transmissionObserver.OnCompleted();
            observer.OnCompleted();
        }
        // Error resulting from bad client input. Report and terminate.
        catch (ClientException exception) {

            this.connectionStatusObservable.CurrentValue = IConnectionStatus.Exception;
            observer.OnError(exception);
        }
        // Force close from client end.
        catch (IOException exception) {

            this.connectionStatusObservable.CurrentValue = IConnectionStatus.Exception;
            observer.OnError(exception);
        }
        catch (Exception exception) {
            this.connectionStatusObservable.CurrentValue = IConnectionStatus.Exception;
            observer.OnError(exception);
        }
        finally {
            await reader.CompleteAsync();
            DisposeConnection();
        }
    }

    /// <summary>Replaces all Bogouscoin address in <paramref name="line"/> with the Bogouscoin address
    /// specified by <see cref="MobProxyClientOptions.ReplacementAddress"/></summary>
    /// <param name="line">The line to inspect and replace addresses in.</param>
    /// <returns>The line with any Bogouscoin addresses replaced.</returns>
    ReadOnlySequence<byte> ReplaceAddresses(ReadOnlySequence<byte> line)
    {
        AppendableSequence<byte> workingSequence = new();
        ReadOnlySequence<byte> workingSegment = line;
        SequencePosition position = line.Start, lastAppendPosition = line.Start;
        SequencePosition? delimiterPosition;

        while ((delimiterPosition = workingSegment.PositionOf(WORD_DELIMITER)) is not null) {
            // split into [workingSegmentStart, delimiter), [delimiter, workingSegmentEnd]
            (var segment, workingSegment) = workingSegment.Split(delimiterPosition.Value);

            if (IsAddress(segment)) {
                // Append the segment before the address if any
                var precedingSegment = line.Slice(lastAppendPosition, segment.Start);
                if (!precedingSegment.IsEmpty) workingSequence.Append(precedingSegment);

                workingSequence.Append(this.options.ReplacementAddress).Append(WordDelimiterMemory);

                // update the last appended position to be past the delimiter
                lastAppendPosition = line.GetPosition(1, delimiterPosition.Value);
            }

            // Slice past the delimiter for the next iteration
            workingSegment = workingSegment.Slice(1);
            position = workingSegment.Start;
        }

        // Handle the trailing segment.
        var finalSegment = line.Slice(lastAppendPosition, position);
        workingSequence.Append(finalSegment);

        // last character should always be end of line here. Trim it for comparison.
        return IsAddress(workingSegment.TrimEnd(1)) ? workingSequence.Append(this.options.ReplacementAddress).Append(LineDelimiterMemory)
                                                    : workingSequence.Append(workingSegment);
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
        this.totalBytesTransmittedObservable.CurrentValue += bytesTransmitted;

        Transmission transmission = new(){
            Data = data.ToArray(),
            Translation = Encoding.ASCII.GetString(data),
        };
        this.transmissionObserver.OnNext(DataTransmissionEvent.FromClient(this, transmission));
    }

    const int MIN_ADDRESS_LENGTH = 26;
    const int MAX_ADDRESS_LENGTH = 35;
    const byte ADDRESS_START_BYTE = (byte) '7';

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