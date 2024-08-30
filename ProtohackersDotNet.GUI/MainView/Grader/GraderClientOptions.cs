namespace ProtoHackersDotNet.GUI.MainView.Grader;

public sealed class GraderClientOptions
{
    [BaseUrl]
    public required Uri BaseAddress { get; init; }
    public required Uri StatusUrl { get; init; }
    public required Uri SubmitUrl { get; init; }

    [TimeSpanRange(min: "00:00:01")]
    public required TimeSpan PollingInterval { get; init; }
}