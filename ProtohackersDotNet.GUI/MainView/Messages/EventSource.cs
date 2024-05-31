using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;
using ProtoHackersDotNet.Servers.Interface.Client.Events;
using ProtoHackersDotNet.Servers.Interface.Server.Events;
using System.Collections.Immutable;
using System.Reactive.Threading.Tasks;

namespace ProtoHackersDotNet.GUI.MainView.Messages;

public class EventSource<TEvent>(IObservable<TEvent> eventStream, CancellationToken token = default) 
    : IEventSource
{
    public IObservable<TEvent> EventStream { get; } = eventStream;
    public required ImmutableArray<string> SourceNames { get; init; }
    public Task<Unit> Completed { get; } = eventStream.Select(_ => Unit.Default).ToTask(token);
    public required Func<TEvent, MessageVM> Translator { get; init; }

    public static EventSource<ServerEvent> FromServer(IServer server, IObservable<ServerEvent> serverEvents)
        => new(serverEvents) {
            SourceNames = [server.Name.ToString()],
            Translator = MessageVM.FromSeverEvent,
        };

    public static EventSource<ClientEvent> FromClient(IClient client, IObservable<ClientEvent> clientEvents)
        => new(clientEvents) {
            SourceNames = [client.ClientEndPoint.ToString(), client.Server.LocalEndPoint!.ToString()],
            Translator = MessageVM.FromClientEvent,
        };

    public static EventSource<TestEvent> FromTestApi(ProtoHackerApiManager apiManager, IObservable<TestEvent> testEvents)
        => new(testEvents) {
            SourceNames = apiManager.EventSources,
            Translator = MessageVM.FromTestEvent,
        };
}