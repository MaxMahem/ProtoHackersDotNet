using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

/// <summary>Base class for a endpoint view model, with built-in validation observables.</summary>
/// <param name="ip">The ip value to initialize this VM with.</param>
/// <param name="port">The port value to initialize this VM with.</param>
public abstract class EndPointVM
{
    public ValidateableValue<IPAddress?> IP { get; protected set; }
    public ValidateableValue<ushort?> Port { get; }
    public CompositValidatableValue<IPAddress?, ushort?, IPEndPoint?> EndPoint { get; protected set; }

    public EndPointVM(SerializableEndPoint serializableEndPoint)
    {
        _ = IPAddress.TryParse(serializableEndPoint.IP, out IPAddress? ip);

        IP = ValidateableValue<IPAddress?>.NotNull(ip);
        Port = ValidateableValue<ushort?>.NotNull(serializableEndPoint.Port);

        EndPoint = new(IP, Port, TryBuildEndPoint);

        static IPEndPoint? TryBuildEndPoint(IPAddress? ip, ushort? port)
            => ip is not null && port is not null ? new IPEndPoint(ip, port.Value) : null;
    }

    /// <summary>Reports whenever the endpoint state changes.</summary>
    public IObservable<SerializableEndPoint> StateChanges => Observable.CombineLatest(IP.Changes, Port.Changes,
        (ip, port) => new SerializableEndPoint() { IP = ip?.ToString(), Port = port })
        .DistinctUntilChanged();

    /// <summary>Serializes this endpoint in a format suitable for appsettings.json export.</summary>
    /// <returns>A serializable representation of this endpoint's values.</returns>
    public SerializableEndPoint ToSerializable() => new()
    {
        IP = IP.Value?.ToString(),
        Port = Port.Value
    };
}