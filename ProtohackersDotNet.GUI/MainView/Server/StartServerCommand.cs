using ProtoHackersDotNet.GUI.Helpers;
using ProtoHackersDotNet.GUI.MainView.EndPoint;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class StartServerCommand(IObservableValue<ServerVM?> serverVM, EndPointVM localEndPoint) 
    : IObservableCommand<StartServerResult>
{
    readonly Subject<StartServerResult> startServerResults = new();

    public IObservable<bool> CanExecute { get; } = Observable.CombineLatest(
            serverVM.Changes.SelectMany(server => server?.Server.Listening ?? Observable.Return(false)),
            localEndPoint.EndPoint.Validity,
            (executing, valid) => !executing && valid
        ).DistinctUntilChanged();

    public IObservable<StartServerResult> Results => this.startServerResults.AsObservable();

    public void Execute()
    {
        var server = serverVM.Value?.Server ?? ThrowArgumentNullException<IServer>();
        var endPoint = localEndPoint.EndPoint.Value ?? ThrowArgumentNullException<IPEndPoint>();

        var serverEvents = server.Start(endPoint);
        this.startServerResults.OnNext(new (server, serverEvents));
    }
}

public readonly record struct StartServerResult(IServer Server, IConnectableObservable<IEvent> Events);