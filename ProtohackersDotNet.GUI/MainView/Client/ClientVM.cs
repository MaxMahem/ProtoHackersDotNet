namespace ProtoHackersDotNet.GUI.MainView.Client;

public class ClientVM(IClient client, TimeSpan updateInterval)
{
    public IClient Client { get; } = client;

    public IObservable<TimeSpan> ConnectionAge { get; }
        = Observable.Interval(updateInterval).Select(_ => client.ConnectedAt - DateTime.UtcNow).StartWith(TimeSpan.Zero)
                    .TakeWhile(_ => client.LatestConnectionStatus is ConnectionStatus.Connected)
                    .Replay(1).RefCount();

    public IObservable<bool> IsConnected { get; } = client.ConnectionStatus.Contains(ConnectionStatus.Connected);
    public IObservable<bool> IsDisconnected { get; } = client.ConnectionStatus.Contains(ConnectionStatus.Disconnected);
    public IObservable<bool> IsTerminated { get; } = client.ConnectionStatus.Contains(ConnectionStatus.Terminated);
}