using ProtoHackersDotNet.Servers.Interface.Client.Events;
using ProtoHackersDotNet.Servers.Interface.Client.Messages;

namespace ProtoHackersDotNet.Servers.Interface.Client;

public interface IClient : IDisposable
{
    /// <summary>Server this client is a child of.</summary>
    IServer Server { get; }

    /// <summary>Id which uniquely identifies this client.</summary>
    Guid Id { get; }

    /// <summary>The remote location this client connects to.</summary>
    IPEndPoint ClientEndPoint { get; }
    ConnectionStatus LatestConnectionStatus { get; }
    IObservable<ConnectionStatus> ConnectionStatus { get; }
    IObservable<string?> Status { get; }
    IObservable<ByteSize> TotalBytesReceived { get; }
    IObservable<ByteSize> TotalBytesTransmitted { get; }

    IObservable<ClientEvent> Events { get; }

    /// <summary>Transmits <paramref name="message"/> to the connected client and notifies subscribers of the event.</summary>
    /// <returns>A <see cref="Task"/> that indicates completion of the transmission.</returns>
    Task Transmit(ITransmission message);
}