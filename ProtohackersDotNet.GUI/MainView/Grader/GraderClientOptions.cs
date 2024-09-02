namespace ProtoHackersDotNet.GUI.MainView.Grader;

public sealed class GraderClientOptions
{
    /// <summary>Base url to access to Protohacker interface from.</summary>
    /// <remarks>Must be a absolute url, ending with a '/'.</remarks>
    [BaseUrl]
    public required Uri BaseAddress { get; init; }

    /// <summary>Url to use to check status of a grading request.</summary>
    public required Uri StatusUrl { get; init; }

    /// <summary>Url to use to submit a grading request.</summary>
    public required Uri SubmitUrl { get; init; }

    /// <summary>Time to wait between polls of the the Protohacker interface.</summary>
    /// <remarks>Minimum interval 1 second.</remarks>
    [TimeSpanRange(min: "00:00:01")]
    public required TimeSpan PollingInterval { get; init; }
}