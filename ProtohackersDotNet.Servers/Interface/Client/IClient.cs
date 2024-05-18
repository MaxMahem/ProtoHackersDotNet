namespace ProtoHackersDotNet.Servers.Interface.Client;

public interface IClient : IDisposable
{
    /// <summary>Id which uniquely identifies this client.</summary>
    Guid Id { get; }

    /// <summary>The remote location this client connects to.</summary>
    EndPoint? RemoteEndPoint { get; }
    DateTime ConnectedAt { get; }
    IObservable<ConnectionStatus> ConnectionStatusChanges { get; }
    ConnectionStatus ConnectionStatus { get; }
    IObservable<string?> StatusChanges { get; }
    string? StatusExtended { get; }
    IObservable<ByteSize> TotalBytesRecievedChanges { get; }
    IObservable<ByteSize> TotalBytesTransmittedChanges { get; }

    event EventHandler<DataTransmission>? DataTransmitted;

    event EventHandler<DataReciept>? DataRecieved;

    event EventHandler<Exception>? Exception;

    /// <summary>Transmits <paramref name="message"/> to the connected client and notifies subscribers of the event.</summary>
    /// <returns>A <see cref="Task"/> that indicates completion of the transmission.</returns>
    Task Transmit(ITransmission message);

    /// <summary>Handles all incoming data for this client.</summary>
    /// <returns>A Task that indicates the client has disconnected, </returns>
    Task HandleClient();
}