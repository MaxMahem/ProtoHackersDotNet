namespace ProtoHackersDotNet.Servers.Interface.Server;

/// <summary>A server that wraps engagement on a ProtoHackers problem.</summary>
/// <typeparam name="TClient">The client type that handles the local end of the ProtoHacker connection.</typeparam>
public interface IServer<out TClient> : IDisposable
    where TClient : IClient
{
    /// <summary>The static name of this server.</summary>
    abstract static string ServerName { get; }

    /// <summary>The Id of the problem this server corresponds to.</summary>
    abstract static int ProblemId { get; }

    /// <summary>The description/problem statement this server corresponds with.
    /// Written in markdown.</summary>
    abstract static string Description { get; }

    /// <summary>Creates a new instance of this server.</summary>
    /// <param name="ip">The ip this server should listen on.</param>
    /// <param name="port">The port this server should listen on.</param>
    /// <returns>A new instance of this server.</returns>
    abstract static IServer<TClient> Create(IPAddress ip, ushort port);
    
    /// <summary>The endpoint this server is listening to.</summary>
    EndPoint EndPoint { get; }

    /// <summary>If this server is running or not.</summary>
    bool Running { get; }

    /// <summary>Clients currently connected to this server.</summary>
    IEnumerable<TClient> Clients { get; }

    event EventHandler<NewClient>? RemoteConnect;
    event EventHandler<RemoteDisconnect>? RemoteDisconnect;

    event EventHandler<ServerMessage>? ServerEvent;

    Task Start(CancellationToken? token = null);
    Task Stop();
}
