using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

/// <summary>Base class for a endpoint view model, with built-in validation observables.</summary>
/// <param name="ip">The ip value to initialize this VM with.</param>
/// <param name="port">The port value to initialize this VM with.</param>
public abstract class EndPointVM
{
    public ValidateableValue<IPAddress?> IP { get; protected set; }
    public ValidateableValue<ushort?> Port { get; protected set; }
    public CompositValidatableValue<IPAddress?, ushort?, IPEndPoint?> EndPoint { get; protected set; }

    public EndPointVM(IPAddress? ip, ushort? port)
    {
        IP = ValidateableValue<IPAddress?>.NotNull(ip);
        Port = ValidateableValue<ushort?>.NotNull(port);
        EndPoint = new(IP, Port, TryBuildEndPoint);

        static IPEndPoint? TryBuildEndPoint(IPAddress? ip, ushort? port)
            => ip is not null && port is not null ? new IPEndPoint(ip, port.Value) : null;
    }


    /// <summary>Serializes this endpoint in a format suitable for appsettings.json export.</summary>
    /// <returns>A serializable representation of this endpoint's values.</returns>
    public SerializableEndPoint ToSerializable() => new()
    {
        IP = IP.Value?.ToString(),
        Port = Port.Value
    };
}