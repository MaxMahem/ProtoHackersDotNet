namespace ProtoHackersDotNet.Servers.Interface.Server;

/// <summary>Factory for creating <see cref="IServer{TClient}"/>s.</summary>
public sealed class ServerFactory<TService> : IServerFactory where TService : IServer<IClient>
{
    public string Name => TService.ServerName;

    public int ProblemId => TService.ProblemId;

    public string Description => TService.Description;

    public IServer<IClient> Create(IPAddress address, ushort port) => TService.Create(address, port);
}
