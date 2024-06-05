using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.Server;

/// <summary>State object for <seealso cref="ServerManager"/>.</summary>
public class ServerManagerState : IState
{
    [JsonIgnore]
    public string ObjectName => nameof(ServerManagerState);

    public SerializableEndPoint? LocalEndPoint { get; init; }
    public SerializableEndPoint? RemoteEndPoint { get; init; }
    public string? Server { get; init; }

    public JsonElement Serialize()
    {
        var json = JsonSerializer.Serialize(this);
        return JsonDocument.Parse(json).RootElement.Clone();
    }
}