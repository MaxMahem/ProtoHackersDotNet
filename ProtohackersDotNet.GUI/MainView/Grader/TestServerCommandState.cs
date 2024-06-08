using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

public class TestServerCommandState : IState
{
    [JsonIgnore]
    public string ObjectName => nameof(TestServerCommandState);
    public SerializableEndPoint? RemoteEndPoint { get; init; }
}