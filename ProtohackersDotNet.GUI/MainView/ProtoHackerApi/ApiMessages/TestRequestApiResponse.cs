namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi.ApiMessages;

public class TestRequestApiResponse : IApiResponse
{
    public ResponseStatus Status { get; init; }

    public required int SubmissionId { get; init; }
}