using System.Reactive.Disposables;
using System.Reactive.Subjects;
using ProtoHackersDotNet.Servers;

namespace ProtoHackersDotNet.GUI.MainView.Server;

/// <summary>Dummy server for design mockup.</summary>
class MockupServer : IServer
{
    public ServerName Name { get; } = ServerName.From("DummyServer");

    public Problem Solution => Problem.Unknown;

    public IPEndPoint? LocalEndPoint { get; } = IPEndPoint.Parse("123.123.123.123:1234");

    public IObservable<bool> Listening => Observable.Return(false);
    public bool CurrentlyListening => false;

    public IObservable<string?> Status => Observable.Return("Status");

    public IObservable<ServerStatus> ServerStatus => Observable.Return(Servers.Interface.Server.ServerStatus.Stopped);

    public void Dispose() { }
    public IConnectableObservable<IEvent> Start(IPEndPoint endpoint, CancellationToken token = default) => throw new NotImplementedException();
    public Task<IDisposable> Stop() => Task.FromResult(Disposable.Empty);

    public readonly static MockupServer Default = new();
}