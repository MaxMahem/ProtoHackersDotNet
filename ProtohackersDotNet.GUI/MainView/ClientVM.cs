namespace ProtoHackersDotNet.GUI.MainView;

public class ClientVM(IClient client)
{
    static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);

    public IClient Client { get; } = client;

    public IObservable<TimeSpan> ConnectionAge { get; }
        = Observable.Interval(UpdateInterval).Select(_ => client.ConnectedAt - DateTime.UtcNow)
                    .TakeWhile(_ => client.ConnectionStatus is ConnectionStatus.Connected)
                    .StartWith(TimeSpan.Zero).Replay(1).RefCount();

    public IObservable<bool> IsConnected { get; } = client.ConnectionStatusChanges.Select(status => status == ConnectionStatus.Connected);
    public IObservable<bool> IsDisconnected { get; } = client.ConnectionStatusChanges.Select(status => status == ConnectionStatus.Disconnected);
    public IObservable<bool> IsTerminated { get; } = client.ConnectionStatusChanges.Select(status => status == ConnectionStatus.Terminated);
}