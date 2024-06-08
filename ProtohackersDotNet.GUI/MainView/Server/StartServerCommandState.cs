using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.Server;

public class StartServerCommandState : IState
{
    [JsonIgnore]
    public string ObjectName => nameof(StartServerCommandState);

    public SerializableEndPoint? LocalEndPoint { get; init; }
}