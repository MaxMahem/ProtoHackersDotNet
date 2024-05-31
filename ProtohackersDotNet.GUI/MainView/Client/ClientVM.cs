namespace ProtoHackersDotNet.GUI.MainView.Client;

public class ClientVM(IClient client)
{
    public IClient Client { get; } = client;

    readonly DateTimeOffset connectedAt = DateTimeOffset.UtcNow;

    [SetsRequiredMembers]
    public ClientVM(IClient client, TimeSpan updateInterval) : this(client)
    {
        ConnectionAge = Observable.Interval(updateInterval).Select(_ => this.connectedAt - DateTime.UtcNow).StartWith(TimeSpan.Zero)
                    .TakeWhile(_ => client.LatestConnectionStatus is ConnectionStatus.Connected)
                    .Replay(1).RefCount();
    }

    public required IObservable<TimeSpan> ConnectionAge { get; init; }
        

    public IObservable<bool> IsConnected { get; } = client.ConnectionStatus.Select(status => status is ConnectionStatus.Connected);
    public IObservable<bool> IsDisconnected { get; } = client.ConnectionStatus.Select(status => status is ConnectionStatus.Disconnected);
    public IObservable<bool> IsTerminated { get; } = client.ConnectionStatus.Select(status => status is ConnectionStatus.Terminated);
}