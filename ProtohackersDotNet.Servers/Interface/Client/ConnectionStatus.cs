﻿namespace ProtoHackersDotNet.Servers.Interface.Client;

/// <summary>Possible status of a client connection.</summary>
public enum ConnectionStatus
{
    /// <summary>The client is connected normally.</summary>
    Connected,
    /// <summary>The client was disconnected gracefully.</summary>
    Disconnected,
    /// <summary>The client was disconnected forcefully.</summary>
    Terminated,
    /// <summary>Error in client code.</summary>
    Error,
}