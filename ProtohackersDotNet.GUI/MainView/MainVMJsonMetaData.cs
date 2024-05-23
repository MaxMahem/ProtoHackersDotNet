using System.Text.Json.Serialization;

namespace ProtoHackersDotNet.GUI.MainView;

[JsonSerializable(typeof(MainViewModelHolder))]
internal partial class MainVMJsonMetaData : JsonSerializerContext;