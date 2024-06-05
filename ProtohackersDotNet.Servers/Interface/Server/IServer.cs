namespace ProtoHackersDotNet.Servers.Interface.Server;

public interface IServer : IDisposable
{
    /// <summary>The name of this server.</summary>
    ServerName Name { get; }

    /// <summary>The problem this server solves.</summary>
    Problem Solution { get; }

    /// <summary>The endpoint this server is listening to.</summary>
    IPEndPoint? LocalEndPoint { get; }

    /// <summary>Provides the current status of the servers listening state.</summary>
    IObservable<bool> Listening { get; }

    /// <summary>Provides an optional text status detailing the current status of the server.</summary>
    IObservable<string?> Status { get; }

    /// <summary>Provides an optional text status detailing the current status of the server.</summary>
    IObservable<ServerStatus> ServerStatus { get; }

    /// <summary>Starts the server and begins listening for connections.</summary>
    /// <param name="endpoint">The IP endpoint the server should listen to.</param>
    /// <param name="token">An optional cancellation token which can be used to shut down the server.</param>
    /// <returns>A stream of events occurring on the server.</returns>
    IConnectableObservable<IEvent> Start(IPEndPoint endpoint, CancellationToken token = default);

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

public enum ServerStatus
{
    /// <summary>Indicates the server was shutdown properly, or has never been started.</summary>
    Stopped,
    /// <summary>Indicates the server is started and listening.</summary>
    Listening,
    /// <summary>Indicates the server was terminated due to an error.</summary>
    Terminated,
}