using ProtoHackersDotNet.GUI.MainView.Grader;

namespace ProtoHackersDotNet.GUI.Serialization;

/// <summary>Represents options for the <see cref="AppState"/> class.</summary>
public class StateSerializationOptions
{
    public string SavePath { get; init; } = default!;

    /// <param name="SaveThrottle">Gets the time that <see cref="AppState"/> should wait for a new change before saving again.</param>
    /// <remarks>Minimum value, 1 second.</remarks>
    [TimeSpanRange(min: "00:00:01")]
    public TimeSpan SaveThrottle { get; init; }
}
