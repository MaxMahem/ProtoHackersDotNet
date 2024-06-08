using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.Server;

/// <summary>State object for <seealso cref="ServerManager"/>.</summary>
public class ServerManagerState : IState
{
    [JsonIgnore]
    public string ObjectName => nameof(ServerManagerState);

    public string? Server { get; init; }
    public string? Problem { get; init; }
}