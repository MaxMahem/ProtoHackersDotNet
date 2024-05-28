namespace ProtoHackersDotNet.Servers.Interface.Server;

public interface IServer : IDisposable
{
    /// <summary>The name of this server.</summary>
    ServerName Name { get; }

    /// <summary>The problem this server solves.</summary>
    Problem Problem { get; }

    /// <summary>The endpoint this server is listening to.</summary>
    IPEndPoint? LocalEndPoint { get; }

    /// <summary>Checks if this server is listening for connections or not.</summary>
    bool CurrentlyListening { get; }

    /// <summary>Provides the current status of the servers listening state.</summary>
    IObservable<bool> Listening { get; }

    /// <summary>Starts the server and begins listening for connections.</summary>
    /// <param name="endpoint">The IP endpoint the server should listen to.</param>
    /// <param name="token">An optional cancellation token which can be used to shut down the server.</param>
    /// <returns>A stream of events occurring on the server.</returns>
    IConnectableObservable<ServerEvent> Start(IPEndPoint endpoint, CancellationToken token = default);

    /// <summary>Stops the server and ends listening for connections.</summary>
    /// <returns>A task representing completion of the stop procedure, and any resources needing disposing.</returns>
    Task<IDisposable> Stop();
}

/// <summary>A server that wraps engagement on a ProtoHackers problem.</summary>
public interface IServer<out TClient> : IServer 
    where TClient : IClient
{
    /// <summary>Clients currently connected to this server.</summary>
    IEnumerable<TClient> Clients { get; }
}