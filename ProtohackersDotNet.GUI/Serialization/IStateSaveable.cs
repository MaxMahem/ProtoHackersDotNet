using ProtoHackersDotNet.GUI.MainView.Server;

namespace ProtoHackersDotNet.GUI.Serialization;

public interface IStateSaveable
{
    IState GetState();
}

/// <summary>Marker interface for serialization.</summary>
[JsonDerivedType(typeof(ServerManagerState))]
public interface IState
{
    [JsonIgnore]
    string ObjectName { get; }
}