﻿using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.ApiMessages;

namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;

/// <summary>Encapsulates interface with the ProtoHackersApi</summary>
/// <param name="client">Client used to call ProtoHacker api. Uses cookies and should live as long as a session does.</param>
public class ProtoHackerApiClient(HttpClient client, IOptions<ProtoHackerApiClientOptions> options)
{
    public Uri SubmitTestUrl { get; } = options.Value.SubmitTestUrl; 
    public Uri TestStatusUrl { get; } = options.Value.TestStatusUrl;

    readonly TimeSpan pollingInterval = options.Value.PollingInterval;

    /// <summary>The last url that was accessed, for error checking.</summary>
    public Uri? LastAccessedUrl { get; private set; }

    /// <summary>Polls the ProtoHacker test server for a response to our test request. Waiting between each request 
    /// to not flood the server.</summary>
    /// <returns>An observable that tracks responses from the server.</returns>
    public IObservable<ApiCheckResponse> PollTestStatus()
    {
        LastAccessedUrl = TestStatusUrl;
        return Observable.FromAsync((token) => client.GetFromJsonAsync(TestStatusUrl, ApiTestsMetadata.Default.ApiCheckResponse, token))
                         .Select(response => response?.Status is ResponseStatus.Ok ? response : ThrowResponseError<ApiCheckResponse>())
                         .Repeat().Delay(this.pollingInterval).TakeWhileInclusive(response => response?.CheckStatus is CheckStatus.Checking);
    }

    /// <summary>Requests testing of a given service from the ProtoHacker API.</summary>
    /// <param name="apiTestRequest"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async Task<TestRequestApiResponse> RequestTesting(IServer server, IPEndPoint endPoint, CancellationToken token)
    {
        LastAccessedUrl = SubmitTestUrl;
        TestApiRequest apiTestRequest = new(){
            Problem = server.Problem.Id,
            Ip = endPoint.Address,
            Port = (ushort) endPoint.Port,
        };

        var requestHttpResponse = await client.PostAsJsonAsync(SubmitTestUrl, apiTestRequest,
                ApiTestsMetadata.Default.TestApiRequest, token);
        var requestApiResponse = await requestHttpResponse.EnsureSuccessStatusCode().Content.ReadFromJsonAsync(
                ApiTestsMetadata.Default.TestRequestApiResponse, token);

        return requestApiResponse?.Status is ResponseStatus.Ok ? requestApiResponse : ThrowResponseError<TestRequestApiResponse>();
    }

    [DoesNotReturn]
    public static T ThrowResponseError<T>() => throw new HttpRequestException("Error response from api");
}