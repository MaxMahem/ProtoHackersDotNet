using ProtoHackersDotNet.GUI.MainView.Grader.Events;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class ServerVM(IServer server)
{
    public IServer Server { get; } = server;
    public string Name => Server.Name.Value;

    public IObservable<bool> Listening => Server.Listening;

    readonly BehaviorSubject<Grade> observableTestResult = new(Grade.Untested);
    public IObservable<Grade> TestResults => this.observableTestResult.AsObservable();

    readonly BehaviorSubject<string?> observableLastError = new(null);
    public IObservable<string?> LastError => this.observableLastError.AsObservable();

    public IObservable<bool> TestSuccessful => TestResults.Select(grade => grade is Grade.Success);
    public IObservable<bool> TestFailure => TestResults.Select(grade => grade is Grade.Failure);
    public IObservable<bool> HasResults => TestResults.Select(grade => grade is not Grade.Untested);

    public void ObserveTest(IObservable<IEvent> testEvents) 
    {
        testEvents.Where(message => message.MessageType is not MessageType.Notice)
                  .Select(message => message.Message).Subscribe(observableLastError).DiscardUnsubscribe();
        testEvents.OfType<GradingResultEvent>().Select(GetGradingResult)
                  .Subscribe(this.observableTestResult.OnNext).DiscardUnsubscribe();
    }

    public static Grade GetGradingResult(GradingResultEvent gradingResultEvent) => gradingResultEvent.MessageType switch {
        MessageType.Success => Grade.Success,
        MessageType.Error   => Grade.Failure,
        _ => ThrowArgumentOutOfRangeException<Grade>("Result events should be either success or failures")
    };

    public static ServerVM Create(IServer server) => new(server);
}