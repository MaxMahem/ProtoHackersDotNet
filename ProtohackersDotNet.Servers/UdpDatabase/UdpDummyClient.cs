namespace ProtoHackersDotNet.Servers.UdpDatabase;

public sealed class UdpDummyClient(UdpDatabaseServer server, IPEndPoint endpoint, ByteSize received, ByteSize transmitted) : IClient
{
    public IServer Server => server;

    public Guid Id => Guid.NewGuid();

    public IPEndPoint LocalEndPoint => endpoint;
    public IPEndPoint ClientEndPoint => endpoint;

    public ConnectionStatus LatestConnectionStatus => Interface.Client.ConnectionStatus.Disconnected;

    public IObservable<ConnectionStatus> ConnectionStatus => Observable.Return(Interface.Client.ConnectionStatus.Disconnected);

    public IObservable<string?> Status => Observable.Return<string?>(null);

    public IObservable<ByteSize> TotalBytesReceived => Observable.Return(received);

    public IObservable<ByteSize> TotalBytesTransmitted => Observable.Return(transmitted);

    public IObservable<IEvent> Events => Observable.Empty<IEvent>();

    public void Dispose() { }
    public Task Transmit(ITransmission message, CancellationToken token = default) 
        => throw new NotImplementedException();
}