using ByteSizeLib;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using IConnectionStatus = ProtoHackersDotNet.Servers.Interface.Client.ConnectionStatus;

namespace ProtoHackersDotNet.GUI.MainView.Client;

public class ClientVM : IComparable<ClientVM>
{
    readonly DateTimeOffset connectedAt = DateTimeOffset.UtcNow;

    readonly ReplaySubject<TimeSpan> connectionAgeSubject = new(1);

    readonly CompositeDisposable disposables;

    readonly IClient client;

    public ClientVM(IClient client, TimeSpan updateInterval)
    {
        this.client = client;
        this.disposables = [
            client,
            Observable.Interval(updateInterval).Select(_ => this.connectedAt - DateTime.UtcNow)
                    .TakeWhile(_ => client.LatestConnectionStatus is IConnectionStatus.Connected)
                    .StartWith(TimeSpan.Zero).Subscribe(connectionAgeSubject),
        ];
        Name = client.Status.Select(status => status is null ? client.ClientEndPoint.ToString()
            : $"{client.ClientEndPoint} - {status}");
    }

    public IObservable<string> Status => Observable.Merge(
        this.client.ConnectionStatus.Select(status => status.ToString()),
        this.client.Events.Select<IEvent, string?>(_ => null)
                          .Catch((Exception exception) => Observable.Return(exception.InnerException?.GetType().Name ?? exception.GetType().Name))
                          .WhereNotNull()
    );

    public IObservable<string?> ExceptionText => this.client.Events.Select<IEvent, string?>(_ => null)
                                                     .Catch((Exception exception) => Observable.Return(exception.Message))
                                                     .WhereNotNull();

    public IObservable<TimeSpan> ConnectionAge => this.connectionAgeSubject.AsObservable();
    public IObservable<ByteSize> TotalBytesTransmitted => this.client.TotalBytesTransmitted;
    public IObservable<ByteSize> TotalBytesReceived => this.client.TotalBytesReceived;
    // public IObservable<IConnectionStatus> ConnectionStatus => this.client.ConnectionStatus;
    public IObservable<string> Name { get; }

    public bool IsConnected 
        => this.client.ConnectionStatus.Select(status => status is IConnectionStatus.Connected).Latest()
                                       .FirstOrDefault(false);
    public IObservable<bool> IsDisconnected 
        => this.client.ConnectionStatus.Select(status => status is IConnectionStatus.Disconnected);
    public IObservable<bool> IsTerminated
        => this.client.ConnectionStatus.Select(status => status is IConnectionStatus.Exception);

    public int CompareTo(ClientVM? other) => other is null ? -1 : this.connectedAt.CompareTo(other.connectedAt);
    public void Dispose() => this.disposables.Dispose();
}