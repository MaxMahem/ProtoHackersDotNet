namespace ProtoHackersDotNet.GUI.MainView.ApiTest.ApiMessages;

public class TestRequestApiResponse : IApiResponse
{
    public ResponseStatus Status { get; init; }

    public required int SubmissionId { get; init; }
}