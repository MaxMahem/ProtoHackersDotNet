using System.Reactive.Subjects;
using ProtoHackersDotNet.Servers.Interface.Exceptions;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.ApiMessages;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.Events;
using ProtoHackersDotNet.Servers.Interface;
using System.Collections.Immutable;

namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;

/// <summary>Manages testing against the ProtoHacker api.</summary>
/// <param name="client">Client that encapsulates getting the http interface.</param>
public class ProtoHackerApiManager(ProtoHackerApiClient client)
{
    /// <summary>Provides the current status of testing.</summary>
    public IObservable<ApiTestStatus> TestingStatus => testingStatusSubject.AsObservable();
    readonly BehaviorSubject<ApiTestStatus> testingStatusSubject = new(ApiTestStatus.NotRunning);
    
    /// <summary>The latest status of testing.</summary>
    public ApiTestStatus LatestTestingStatus {
        get => this.testingStatusSubject.Value;
        private set => this.testingStatusSubject.OnNext(value);
    }

    public ImmutableArray<string> EventSources { get; } = [client.SubmitTestUrl.ToString(), client.TestStatusUrl.ToString()];

    /// <summary>Tests <paramref name="server"/> against the ProtoHacker testing API.</summary>
    /// <param name="server">The server to test.</param>
    /// <param name="remoteEndPoint">The end point the ProtoHacker API should test against.</param>
    /// <returns>A live stream of testing events.</returns>
    public IObservable<TestEvent> TestServer(IServer server, IPEndPoint remoteEndPoint)
        => Observable.Create<TestEvent>(async (observer, token) => await ObserveTest(server, remoteEndPoint, observer, token));

    /// <summary>Observes and reports on the progress of the tests.</summary>
    /// <param name="server">The server to test.</param>
    /// <param name="remoteEndPoint">The end point the ProtoHacker API should test against.</param>
    /// <param name="observer">Observer to report back results to.</param>
    /// <param name="token">A cancellation token called in the event of unsubscription.</param>
    /// <returns>A task that represents completion of the tests.</returns>
    async Task ObserveTest(IServer server, IPEndPoint remoteEndPoint, IObserver<TestEvent> observer, CancellationToken token)
    {
        try {
            LatestTestingStatus = ApiTestStatus.Running;
            observer.OnNext(new TestRequestEvent(server, client.SubmitTestUrl));

            TestRequestApiResponse requestApiResponse = await client.RequestTesting(server, remoteEndPoint, token);
            observer.OnNext(new TestRequestResponse(server, client.SubmitTestUrl, requestApiResponse));

            ApiLogParser logProcessor = new(server, client.TestStatusUrl);

            // repeatedly poll the server for testing status
            ApiCheckResponse lastResponse = await client.PollTestStatus().Do(response => {
                foreach (var logEvent in logProcessor.ProcessLogs(response))
                    observer.OnNext(logEvent);
            }).TakeUntil(_ => token.IsCancellationRequested).LastAsync();

            // report final status.
            observer.OnNext(new TestResultEvent(server, client.TestStatusUrl, lastResponse));
            LatestTestingStatus = lastResponse.CheckStatus is CheckStatus.Pass ? ApiTestStatus.Pass : ApiTestStatus.Fail;
        }
        catch (Exception exception) {
            LatestTestingStatus = ApiTestStatus.Fail;
            observer.OnError(ProtoHackerApiException.FromException(server, client.LastAccessedUrl, exception));
        }
        observer.OnCompleted();
    }

    /// <summary>Encapsulates state for parsing log files from the ProtoHacker check api.</summary>
    /// <remarks><see cref="ApiCheckResponse"/> Log files include past log entries, so this handles
    /// tracking what lines still need to be parsed.</remarks>
    class ApiLogParser(IServer server, Uri checkApi)
    {
        const char LOG_LINE_SEPERATOR = '\n';

        int processedLogLength = 0;

        /// <summary>Processes all new log data from <paramref name="checkResponse"/></summary>
        /// <param name="checkResponse">The response to process.</param>
        /// <returns>An enumeration of all *new* TestLogMessages found in <paramref name="checkResponse"/>.</returns>
        public IEnumerable<TestLogMessage> ProcessLogs(ApiCheckResponse checkResponse)
        {
            var unseenLog = checkResponse.Log[this.processedLogLength..];
            foreach (var line in unseenLog.Split(LOG_LINE_SEPERATOR, StringSplitOptions.RemoveEmptyEntries))
                yield return new TestLogMessage(server, checkApi, line);
            this.processedLogLength = checkResponse.Log.Length;
        }
    }
}
