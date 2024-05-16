namespace ProtoHackersDotNet.Servers.Interface.Server;

/// <summary>Factory for creating <see cref="IServer{TClient}"/>s.</summary>
public interface IServerFactory
{
    /// <summary>The name of the server to be created.</summary>
    string Name { get; }

    /// <summary>The problem id associated with this server.</summary>
    int ProblemId { get; }

    /// <summary>The description/problem statement associated with this server. Written in markdown.</summary>
    string Description { get; }

    /// <summary>Creates a new <see cref="IServer{TClient}"/>.</summary>
    /// <param name="address">The IP address the server should listen on.</param>
    /// <param name="port">The port the server should listen on.</param>
    /// <returns>A new <see cref="IServer{TClient}"/>.</returns>
    IServer<IClient> Create(IPAddress address, ushort port);
}