using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.Grader;

public class TestServerCommandState : IState
{
    [JsonIgnore]
    public string ObjectName => nameof(TestServerCommandState);

    public TestServerCommandState Update(SerializableEndPoint endPoint)
    {
        RemoteEndPoint = endPoint;
        return this;
    }

    public SerializableEndPoint? RemoteEndPoint { get; set; }
}