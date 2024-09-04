using ProtoHackersDotNet.GUI.Helpers;
using ProtoHackersDotNet.GUI.MainView.EndPoint;
using ProtoHackersDotNet.GUI.MainView.Grader.Events;
using ProtoHackersDotNet.GUI.MainView.Server;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

public class TestServerCommand(IObservableValue<ServerVM?> serverVM, EndPointVM remoteEndPoint, GradingService gradingService) 
    : IObservableCommand<TestServerResult>
{
    readonly Subject<TestServerResult> gradingEventResults = new();

    public IObservable<bool> CanExecute { get; } = Observable.CombineLatest(
            serverVM.Changes.SelectMany(server => server?.Server.Listening ?? Observable.Return(false)),
            remoteEndPoint.EndPoint.Validity,
            gradingService.Grading,
            (listening, valid, grading) => listening && valid && !grading
        ).DistinctUntilChanged();

    public IObservable<TestServerResult> Results => gradingEventResults.AsObservable();

    public void Execute()
    {
        var selectedServerVM = serverVM.Value ?? ThrowArgumentNullException<ServerVM>();
        var endPoint = remoteEndPoint.EndPoint.Value ?? ThrowArgumentNullException<IPEndPoint>();

        var testEvents = gradingService.GradeServer(selectedServerVM.Server, endPoint);
        TestServerResult result = new TestServerResult(selectedServerVM, testEvents);
        this.gradingEventResults.OnNext(result);
    }
}

public readonly record struct TestServerResult(ServerVM ServerVM, IObservable<GradingEvent> GradingEvents);
