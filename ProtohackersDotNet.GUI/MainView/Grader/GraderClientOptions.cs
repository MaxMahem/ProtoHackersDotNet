namespace ProtoHackersDotNet.GUI.MainView.Grader;

public class GraderClientOptions
{
    public required Uri BaseAddress { get; init; }
    public required Uri StatusUrl { get; init; }
    public required Uri SubmitUrl { get; init; }
    public required TimeSpan PollingInterval { get; init; }
}
