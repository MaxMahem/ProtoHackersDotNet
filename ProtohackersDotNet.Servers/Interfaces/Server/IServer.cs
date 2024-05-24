﻿namespace ProtoHackersDotNet.Servers.Interfaces.Server;

public interface IServer : IDisposable
{
    string Name { get; }

    /// <summary>The problem this server solves.</summary>
    Problem Problem { get; }

    /// <summary>The endpoint this server is listening to.</summary>
    IPEndPoint? EndPoint { get; }

    bool CurrentlyListening { get; }

    /// <summary>If this server is running or not.</summary>
    IObservable<bool> Listening { get; }

    Task Start(IPEndPoint endpoint, CancellationToken? token = null);
    Task Stop();
}

/// <summary>A server that wraps engagement on a ProtoHackers problem.</summary>
public interface IServer<out TClient> : IServer 
    where TClient : IClient
{
    /// <summary>Clients currently connected to this server.</summary>
    IEnumerable<TClient> Clients { get; }

    IObservable<TClient> ClientConnections { get; }
    IObservable<TClient> ClientDisconnections { get; }
    // IObservable<ServerEvent> ServerEvents { get; }
    IObservable<Exception> ServerExceptions { get; }
}