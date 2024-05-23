using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ProtoHackersDotNet.GUI.MainView.ApiTest.ApiRequests;
using ProtoHackersDotNet.GUI.MainView.ApiTest.Messages;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest;

public class ApiTestManager(IHttpClientFactory clientFactory, IOptions<ApiTestManagerOptions> options)
{
    const string CLIENT_NAME = nameof(ApiTestManager);
    readonly ApiTestManagerOptions options = options.Value;

    readonly Subject<TestEvent> testingEventObserver = new();
    public IObservable<TestEvent> TestingEvents => this.testingEventObserver.AsObservable();
    void NotifyTestEvent(TestEvent testEvent) => this.testingEventObserver.OnNext(testEvent);

    public IObservable<bool> TestRunning => TestingStatus.Select(status => status is ApiTestStatus.Running);

    public IObservable<ApiTestStatus> TestingStatus => testingStatusSubject.AsObservable();
    readonly BehaviorSubject<ApiTestStatus> testingStatusSubject = new(ApiTestStatus.NotRunning);
    public ApiTestStatus CurrentTestingStatus {
        get => this.testingStatusSubject.Value;
        private set => this.testingStatusSubject.OnNext(value);
    }

    public async Task CheckProblem(IServer server, IPEndPoint endpoint, CancellationToken token = default)
    {
        TestApiRequest testApiRequest = new(){
            Problem = server.ProblemId,
            Ip = endpoint.Address,
            Port = (ushort) endpoint.Port,
        };
        HttpClient client = clientFactory.CreateClient(CLIENT_NAME);

        try {
            CurrentTestingStatus = ApiTestStatus.Running;
            NotifyTestEvent(new TestRequested(server, this.options.SubmissionUrl, testApiRequest));
            var testResuestHttpResponse = await client.PostAsJsonAsync(this.options.SubmissionUrl, testApiRequest, 
                ApiTestsMetadata.Default.TestApiRequest, token);
            var testRequestApiResponse = await testResuestHttpResponse.EnsureSuccessStatusCode().Content.ReadFromJsonAsync(
                ApiTestsMetadata.Default.TestRequestApiResponse, token);

            if (testRequestApiResponse?.Status is not ResponseStatus.Ok) ThrowResponseError();
            NotifyTestEvent(new TestRequestResponse(server, this.options.SubmissionUrl, testRequestApiResponse));

            ApiLogParser logProcessor = new(server, this.options.CheckUrl);
            ApiCheckResponse? checkResponse;
            do {
                checkResponse = await client.GetFromJsonAsync(this.options.CheckUrl,
                    ApiTestsMetadata.Default.ApiCheckResponse, token);
                if (checkResponse?.Status is not ResponseStatus.Ok)
                    ThrowResponseError();

                foreach (var logEvent in logProcessor.ProcessLogs(checkResponse))
                    NotifyTestEvent(logEvent);

                await Task.Delay(this.options.CheckRetryPeriod, token);
            } while (checkResponse.CheckStatus is CheckStatus.Checking && !token.IsCancellationRequested);

            NotifyTestEvent(new TestResultMessage(server, this.options.CheckUrl, checkResponse));
            CurrentTestingStatus = checkResponse.CheckStatus is CheckStatus.Pass ? ApiTest.ApiTestStatus.Pass : ApiTest.ApiTestStatus.Fail;
        }
        catch (Exception ex) {
            CurrentTestingStatus = ApiTest.ApiTestStatus.Fail;
            Console.Write(ex.ToString());
        }
    }

    [DoesNotReturn]
    public static void ThrowResponseError() => throw new HttpRequestException("Error response from api.");
}
