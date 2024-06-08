using ProtoHackersDotNet.GUI.MainView.Grader.Messages;
using ProtoHackersDotNet.GUI.MainView.Grader.Events;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

/// <summary>Manages testing against the ProtoHacker api.</summary>
/// <param name="client">Client that encapsulates getting the http interface.</param>
public class GradingService(GraderClient client)
{
    readonly ObservableValue<bool> observableIsGrading = new(false);

    /// <summary>Provides the current status of testing.</summary>
    public IObservable<bool> Grading => this.observableIsGrading.Value.AsObservable();

    public ImmutableArray<string> EventSources { get; } = [client.BaseAddress.Host];

    /// <summary>Tests <paramref name="server"/> against the ProtoHacker testing API.</summary>
    /// <param name="server">The server to test.</param>
    /// <param name="remoteEndPoint">The end point the ProtoHacker API should test against.</param>
    /// <returns>A live stream of testing events.</returns>
    public IObservable<GradingEvent> GradeServer(IServer server, IPEndPoint remoteEndPoint)
        => Observable.Create<GradingEvent>(async (observer, token) => await ObserveTest(server, remoteEndPoint, observer, token))
                     .Publish().RefCount();

    /// <summary>Observes and reports on the progress of the tests.</summary>
    /// <param name="server">The server to test.</param>
    /// <param name="remoteEndPoint">The end point the ProtoHacker API should test against.</param>
    /// <param name="observer">Observer to report back results to.</param>
    /// <param name="token">A cancellation token called in the event of unsubscription.</param>
    /// <returns>A task that represents completion of the tests.</returns>
    async Task ObserveTest(IServer server, IPEndPoint remoteEndPoint, IObserver<GradingEvent> observer, CancellationToken token)
    {
        Debug.Assert(!this.observableIsGrading.CurrentValue);

        try {
            this.observableIsGrading.CurrentValue = true;
            observer.OnNext(new GradingRequestEvent(server, client.BaseAddress));

            GradingRequestResponse requestApiResponse = await client.RequestTesting(server, remoteEndPoint, token);
            observer.OnNext(new GradingRequestResponseEvent(server, client.BaseAddress, requestApiResponse, client.PollInterval));

            LogParser logProcessor = new(client.BaseAddress);

            // repeatedly poll the server for testing status
            GraderResponse lastResponse = await client.PollTestStatus().Do(response => {
                foreach (var logEvent in logProcessor.ProcessLogs(response))
                    observer.OnNext(logEvent);
            }).TakeUntil(_ => token.IsCancellationRequested).LastAsync();

            // report final status.
            observer.OnNext(new GradingResultEvent(server, client.BaseAddress, lastResponse));
            observer.OnCompleted();
        }
        catch (Exception exception) {
            observer.OnError(GradingException.FromException(server, client.LastAccessedUrl, exception));
        }
        finally {
            this.observableIsGrading.CurrentValue = false;
        }
    }

    /// <summary>Encapsulates state for parsing log files from the ProtoHacker grader.</summary>
    /// <remarks><see cref="GraderResponse"/> Log files include past log entries, so this handles
    /// tracking what lines still need to be parsed.</remarks>
    class LogParser(Uri checkApi)
    {
        const char LOG_LINE_SEPERATOR = '\n';

        int processedLogLength = 0;

        /// <summary>Processes all new log data from <paramref name="checkResponse"/></summary>
        /// <param name="checkResponse">The response to process.</param>
        /// <returns>An enumeration of all *new* TestLogMessages found in <paramref name="checkResponse"/>.</returns>
        public IEnumerable<GradingLogMessage> ProcessLogs(GraderResponse checkResponse)
        {
            var unseenLog = checkResponse.Log[this.processedLogLength..];
            foreach (var line in unseenLog.Split(LOG_LINE_SEPERATOR, StringSplitOptions.RemoveEmptyEntries))
                yield return new GradingLogMessage(checkApi, line);
            this.processedLogLength = checkResponse.Log.Length;
        }
    }
}
