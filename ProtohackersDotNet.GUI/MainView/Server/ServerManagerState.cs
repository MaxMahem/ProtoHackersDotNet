using ProtoHackersDotNet.GUI.EndPointVM;

namespace ProtoHackersDotNet.GUI.MainView.Server;

/// <summary>State object for <seealso cref="ServerManager"/>.</summary>
public class ServerManagerState
{
    public SerializableEndPoint? LocalEndPoint { get; init; }
    public SerializableEndPoint? RemoteEndPoint { get; init; }
    public string? Server { get; init; }
}