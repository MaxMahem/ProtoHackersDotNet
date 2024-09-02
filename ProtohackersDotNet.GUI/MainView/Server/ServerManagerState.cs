using ProtoHackersDotNet.GUI.Serialization;
using ProtoHackersDotNet.Servers;

namespace ProtoHackersDotNet.GUI.MainView.Server;

/// <summary>State object for <seealso cref="ServerManager"/>.</summary>
public class ServerManagerState : IState
{
    [JsonIgnore]
    public string ObjectName => nameof(ServerManagerState);

    public string? Server { get; set; }
    public string? Problem { get; set; }

    public ServerManagerState Update(ServerVM? server, IProblem problem)
    {
        Server = server?.Server.Name.Value;
        Problem = problem.Name;
        return this;
    }
}