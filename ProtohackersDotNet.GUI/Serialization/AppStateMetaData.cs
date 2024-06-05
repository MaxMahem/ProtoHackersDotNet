using ProtoHackersDotNet.GUI.MainView.Server;

namespace ProtoHackersDotNet.GUI.Serialization;

[JsonSerializable(typeof(Dictionary<string, IState>))]
[JsonSerializable(typeof(ServerManagerState))]
partial class AppStateMetaData : JsonSerializerContext;