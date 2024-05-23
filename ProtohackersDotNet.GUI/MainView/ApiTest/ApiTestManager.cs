using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using System.Linq;
using ProtoHackersDotNet.GUI.MainView.ApiTest.ApiMessages;
using ProtoHackersDotNet.GUI.MainView.ApiTest.Messages;

namespace ProtoHackersDotNet.GUI.MainView.ApiTest;

public class ApiTestManager(IHttpClientFactory clientFactory, IOptions<ApiTestManagerOptions> options)
{
    const string CLIENT_NAME = nameof(ApiTestManager);
    readonly ApiTestManagerOptions options = options.Value;

    public IObservable<bool> TestRunning => TestingStatus.Select(status => status is ApiTestStatus.Running);

    public IObservable<ApiTestStatus> TestingStatus => testingStatusSubject.AsObservable();
    readonly BehaviorSubject<ApiTestStatus> testingStatusSubject = new(ApiTestStatus.NotRunning);
    public ApiTestStatus LastestTestingStatus {
        get => this.testingStatusSubject.Value;
        private set => this.testingStatusSubject.OnNext(value);
    }

    public IObservable<TestEvent> TestProblem(IServer server, IPEndPoint endPoint)
        => Observable.Create<TestEvent>(async (observer, token) => await ObserveTest(server, endPoint, observer, token));

    async Task ObserveTest(IServer server, IPEndPoint endPoint, IObserver<TestEvent> observer, CancellationToken token)
    {
        TestApiRequest apiTestRequest = new(){
            Problem = server.ProblemId,
            Ip = endPoint.Address,
            Port = (ushort) endPoint.Port,
        };
        HttpClient client = clientFactory.CreateClient(CLIENT_NAME);

        try {
            LastestTestingStatus = ApiTestStatus.Running;
            observer.OnNext(new TestRequested(server, this.options.SubmissionUrl, apiTestRequest));

            TestRequestApiResponse requestApiResponse = await RequestTesting(apiTestRequest, client, token);
            observer.OnNext(new TestRequestResponse(server, this.options.SubmissionUrl, requestApiResponse));

            ApiLogParser logProcessor = new(server, this.options.CheckUrl);

            // repeatedly poll the server for testing status
            ApiCheckResponse lastResponse = await PollServer(client, token).AggregateAsync((last, current) => {
                // process the log from each response.
                foreach (var logEvent in logProcessor.ProcessLogs(current))
                    observer.OnNext(logEvent);
                return current; // save the last response
            }, cancellationToken: token);

            // report final status.
            observer.OnNext(new TestResultEvent(server, this.options.CheckUrl, lastResponse));
            LastestTestingStatus = lastResponse.CheckStatus is CheckStatus.Pass ? ApiTestStatus.Pass : ApiTestStatus.Fail;
        }
        catch (Exception exception) {
            LastestTestingStatus = ApiTestStatus.Fail;
            exception.Source = "ApiTesting";
            observer.OnError(exception);
        }
        observer.OnCompleted();
    }

    async IAsyncEnumerable<ApiCheckResponse> PollServer(HttpClient client, [EnumeratorCancellation] CancellationToken token)
    {
        ApiCheckResponse? checkResponse;
        do {
            checkResponse = await client.GetFromJsonAsync(this.options.CheckUrl,
                        ApiTestsMetadata.Default.ApiCheckResponse, token);
            yield return checkResponse?.Status is ResponseStatus.Ok ? checkResponse : ThrowResponseError<ApiCheckResponse>();

            await Task.Delay(this.options.CheckRetryPeriod, token);
        } while (checkResponse.CheckStatus is CheckStatus.Checking && !token.IsCancellationRequested);
    }

    async Task<TestRequestApiResponse> RequestTesting(TestApiRequest apiTestRequest, HttpClient client, CancellationToken token)
    {
        var requestHttpResponse = await client.PostAsJsonAsync(this.options.SubmissionUrl, apiTestRequest,
                ApiTestsMetadata.Default.TestApiRequest, token);
        var requestApiResponse = await requestHttpResponse.EnsureSuccessStatusCode().Content.ReadFromJsonAsync(
                ApiTestsMetadata.Default.TestRequestApiResponse, token);

        return requestApiResponse?.Status is ResponseStatus.Ok ? requestApiResponse : ThrowResponseError<TestRequestApiResponse>();
    }

    [DoesNotReturn]
    public static T ThrowResponseError<T>() => throw new HttpRequestException("Error response from api.");
}
