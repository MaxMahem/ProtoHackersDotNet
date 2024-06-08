using System.Net.Http;
using System.Net.Http.Json;
using ProtoHackersDotNet.GUI.MainView.Grader.Messages;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

/// <summary>Encapsulates interface with the ProtoHackers Grading Api</summary>
/// <param name="client">Client used to call ProtoHacker Grader. Uses cookies and should live as long as a session does.</param>
/// <param name="options">Configuration options for this grader.</param>
public class GraderClient(HttpClient client, GraderClientOptions options)
{
    /// <summary>The base address to grade the server against.</summary>
    public Uri BaseAddress { get; } = options.BaseAddress;
    readonly Uri submitTestUrl = new(options.BaseAddress, options.SubmitUrl); 
    readonly Uri testStatusUrl = new(options.BaseAddress, options.StatusUrl);

    /// <summary>The interval the client should wait before polling the grading server again.</summary>
    public TimeSpan PollInterval { get; } = options.PollingInterval;

    /// <summary>The last url that was accessed, for error checking.</summary>
    public Uri? LastAccessedUrl { get; private set; }

    /// <summary>Polls the ProtoHacker test server for a response to our test request. Waiting between each request 
    /// to not flood the server.</summary>
    /// <returns>An observable that tracks responses from the server.</returns>
    public IObservable<GraderResponse> PollTestStatus()
    {
        LastAccessedUrl = testStatusUrl;
        return Observable.FromAsync((token) => client.GetFromJsonAsync(this.testStatusUrl, GradingMetadata.Default.GraderResponse, token))
                         .Select(response => response?.Status is ResponseStatus.Ok ? response : ThrowResponseError<GraderResponse>())
                         .Repeat().Delay(PollInterval).TakeWhileInclusive(response => response?.CheckStatus is GraderStatus.Checking);
    }

    /// <summary>Requests testing of a given service from the ProtoHacker API.</summary>
    /// <param name="apiTestRequest"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<GradingRequestResponse> RequestTesting(IServer server, IPEndPoint endPoint, CancellationToken token)
    {
        LastAccessedUrl = submitTestUrl;
        GradingRequest apiTestRequest = new(){
            Problem = server.Solution.Number,
            Ip = endPoint.Address,
            Port = (ushort) endPoint.Port,
        };

        var requestHttpResponse = await client.PostAsJsonAsync(submitTestUrl, apiTestRequest,
                GradingMetadata.Default.GradingRequest, token);
        var requestApiResponse = await requestHttpResponse.EnsureSuccessStatusCode().Content.ReadFromJsonAsync(
                GradingMetadata.Default.GradingRequestResponse, token);

        return requestApiResponse?.Status is ResponseStatus.Ok ? requestApiResponse : ThrowResponseError<GradingRequestResponse>();
    }

    [DoesNotReturn]
    public static T ThrowResponseError<T>() => throw new HttpRequestException("Error response from api");
}