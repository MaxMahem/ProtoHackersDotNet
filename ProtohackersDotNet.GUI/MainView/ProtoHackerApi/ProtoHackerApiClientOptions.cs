namespace ProtoHackersDotNet.GUI.MainView.ProtoHackerApi;

public class ProtoHackerApiClientOptions
{
    public required Uri TestStatusUrl { get; init; }
    public required Uri SubmitTestUrl { get; init; }
    public required TimeSpan PollingInterval { get; init; }
}
