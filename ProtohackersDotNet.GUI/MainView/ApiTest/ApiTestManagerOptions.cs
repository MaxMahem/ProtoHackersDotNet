namespace ProtoHackersDotNet.GUI.MainView.ApiTest;

public class ApiTestManagerOptions
{
    public required Uri CheckUrl { get; init; }
    public required Uri SubmissionUrl { get; init; }
    public required TimeSpan CheckRetryPeriod { get; init; }
}
