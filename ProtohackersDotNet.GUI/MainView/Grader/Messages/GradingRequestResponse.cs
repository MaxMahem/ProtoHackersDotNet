namespace ProtoHackersDotNet.GUI.MainView.Grader.Messages;

public class GradingRequestResponse : IGraderApiResponse
{
    public ResponseStatus Status { get; init; }

    public required int SubmissionId { get; init; }
}