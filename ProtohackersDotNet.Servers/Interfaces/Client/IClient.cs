namespace ProtoHackersDotNet.Servers.Interfaces.Client;

public interface IClient : IDisposable
{
    /// <summary>Server this client is a child of.</summary>
    IServer Server { get; }

    /// <summary>Id which uniquely identifies this client.</summary>
    Guid Id { get; }
    /// <summary>The remote location this client connects to.</summary>
    EndPoint EndPoint { get; }
    DateTime ConnectedAt { get; }
    ConnectionStatus CurrentConnectionStatus { get; }
    IObservable<ConnectionStatus> ConnectionStatus { get; }
    IObservable<string?> Status { get; }
    IObservable<ByteSize> TotalBytesRecieved { get; }
    IObservable<ByteSize> TotalBytesTransmitted { get; }

    // IObservable<ClientEvent> Events { get; }

    IObservable<ITransmission> Transmissions { get; }
    IObservable<ITransmission> Receptions { get; }
    IObservable<Exception> Exceptions { get; }

    /// <summary>Transmits <paramref name="message"/> to the connected client and notifies subscribers of the event.</summary>
    /// <returns>A <see cref="Task"/> that indicates completion of the transmission.</returns>
    Task Transmit(ITransmission message);

    /// <summary>Handles all incoming data for this client.</summary>
    /// <returns>A Task that indicates the client has disconnected, </returns>
    Task HandleClient();
}