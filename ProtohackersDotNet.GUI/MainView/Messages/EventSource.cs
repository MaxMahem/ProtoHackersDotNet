using ProtoHackersDotNet.GUI.MainView.Grader;
namespace ProtoHackersDotNet.GUI.MainView.Messages;

public class EventSource(IObservable<IEvent> eventStream, CancellationToken token = default) 
    : IEventSource
{
    public IObservable<IEvent> EventStream { get; } = eventStream;
    public required ImmutableArray<string> SourceNames { get; init; }
    public required MessageSource MessageSource { get; init; }
    public Task<Unit> Completed { get; } = eventStream.Select(_ => Unit.Default).ToTask(token);

    public static EventSource FromServer(IServer server, IObservable<IEvent> serverEvents)
        => new(serverEvents) { 
            SourceNames = [server.Name.ToString()],
            MessageSource = MessageSource.Server,
        };

    public static EventSource FromClient(IClient client, IObservable<IEvent> clientEvents)
        => new(clientEvents) {
            SourceNames = [client.ClientEndPoint.ToString(), client.LocalEndPoint!.ToString()],
            MessageSource = MessageSource.Client,
        };

    public static EventSource FromGrader(GradingService gradingService, IObservable<IEvent> testEvents)
        => new(testEvents) { 
            SourceNames = gradingService.EventSources,
            MessageSource = MessageSource.Grader,
        };
}