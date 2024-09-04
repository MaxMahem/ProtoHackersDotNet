using ProtoHackersDotNet.GUI.MainView;
using ProtoHackersDotNet.GUI.MainView.Server;

namespace ProtoHackersDotNet.GUI.Serialization;

/// <summary>Marker interface for serialization.</summary>
[JsonDerivedType(typeof(ServerManagerState)), JsonDerivedType(typeof(MainViewModelState))]
public interface IState
{
    [JsonIgnore]
    string ObjectName { get; }
}