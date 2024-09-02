using ProtoHackersDotNet.GUI.MainView.Grader;
using ProtoHackersDotNet.GUI.MainView.Server;

namespace ProtoHackersDotNet.GUI.Serialization;

[JsonSerializable(typeof(ReadOnlyDictionary<string, IState>))]
[JsonSerializable(typeof(ServerManagerState))]
[JsonSerializable(typeof(TestServerCommandState))]
[JsonSerializable(typeof(StartServerCommandState))]
partial class AppStateMetaData : JsonSerializerContext;