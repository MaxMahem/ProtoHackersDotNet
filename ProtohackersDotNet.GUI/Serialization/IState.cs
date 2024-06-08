using ProtoHackersDotNet.GUI.MainView.Grader;
using ProtoHackersDotNet.GUI.MainView.Server;

namespace ProtoHackersDotNet.GUI.Serialization;

/// <summary>Marker interface for serialization.</summary>
[JsonDerivedType(typeof(ServerManagerState)), JsonDerivedType(typeof(TestServerCommandState))]
[JsonDerivedType(typeof(StartServerCommandState))]
public interface IState
{
    [JsonIgnore]
    string ObjectName { get; }
}