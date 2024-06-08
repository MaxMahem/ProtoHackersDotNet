using ProtoHackersDotNet.GUI.MainView.EndPoint;
using ProtoHackersDotNet.GUI.MainView.Messages;
using ProtoHackersDotNet.GUI.MainView.Server;
using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

public class TestServerCommand : IStateSaveable, IObservableCommand
{
    readonly ServerManager serverManager;
    readonly MessageManager messageManager;

    public TestServerCommand(GradingService grader, MessageManager messageManager, ServerManager serverManager,
            TestServerCommandState state)
    {
        this.serverManager = serverManager;
        this.messageManager = messageManager;

        Grader = grader;
        RemoteEndPoint = new(
            ip: IPAddress.TryParse(state.RemoteEndPoint?.IP, out var ip) ? ip : null,
            port: state.RemoteEndPoint?.Port
        );
        CanExecute = Observable.CombineLatest(
            this.serverManager.Server.SelectMany(server => server?.Listening ?? Observable.Return(false)),
            RemoteEndPoint.Valid,
            Grader.Grading,
            (listening, valid, grading) => listening && valid && !grading
        ).DistinctUntilChanged();
    }

    public GradingService Grader { get; }
    public TextEndPoint RemoteEndPoint { get; }

    public IObservable<bool> CanExecute { get; }

    public IObservable<bool> Executing => Grader.Grading;

    public void Execute()
    {
        var selectedServerVM = this.serverManager.SelectedServer ?? ThrowArgumentNullException<ServerVM>();
        var remoteEndPoint = RemoteEndPoint.LatestEndPoint ?? ThrowArgumentNullException<IPEndPoint>();

        var testEvents = Grader.GradeServer(selectedServerVM.Server, remoteEndPoint);
        this.messageManager.SubscribeToStream(EventSource.FromGrader(Grader, testEvents));
        selectedServerVM.ObserveTest(testEvents);
    }

    public IState GetState() => new TestServerCommandState() { RemoteEndPoint = RemoteEndPoint.ToSerializable() };
}