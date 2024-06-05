namespace ProtoHackersDotNet.Servers.Interface.Client;

public interface IClient : IDisposable
{
    /// <summary>The location this client connects from.</summary>
    IPEndPoint LocalEndPoint { get; }

    /// <summary>The remote location this client connects to.</summary>
    IPEndPoint ClientEndPoint { get; }

    ConnectionStatus LatestConnectionStatus { get; }
    IObservable<ConnectionStatus> ConnectionStatus { get; }
    IObservable<string?> Status { get; }
    IObservable<ByteSize> TotalBytesReceived { get; }
    IObservable<ByteSize> TotalBytesTransmitted { get; }

    IObservable<IEvent> Events { get; }
}

// public interface IClient
// {
//     /// <summary>Transmits <paramref name="message"/> to the connected client and notifies subscribers of the event.</summary>
//     /// <returns>A <see cref="Task"/> that indicates completion of the transmission.</returns>
//     Task Transmit(ITransmission message, CancellationToken token = default);
// }