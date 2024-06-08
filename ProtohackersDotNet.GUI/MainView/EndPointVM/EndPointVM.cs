using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

/// <summary>Base class for a endpoint view model, with built-in validation observables.</summary>
/// <param name="ip">The ip value to initialize this VM with.</param>
/// <param name="port">The port value to initialize this VM with.</param>
public abstract class EndPointVM(IPAddress? ip, ushort? port)
{
    readonly ValidateableValue<ushort?> validateablePort = ValidateableValue<ushort?>.NotNull(port);
    readonly ValidateableValue<IPAddress?> validateableIP = ValidateableValue<IPAddress?>.NotNull(ip);

    /// <summary>The current port value of this endpoint.</summary>
    /// <remarks>Nullable, because the UI may want to blank it out.</remarks>
    public ushort? Port
    {
        get => validateablePort.CurrentValue;
        set => validateablePort.CurrentValue = value;
    }

    /// <summary>Provides updates on the validity (<c>not null</c>) of <see cref="Port"/>.</summary>
    public IObservable<bool> PortValid => validateablePort.Valid;

    /// <summary>The current IP value of this endpoint.</summary>
    public virtual IPAddress? IP
    {
        get => validateableIP.CurrentValue;
        set => validateableIP.CurrentValue = value;
    }
    /// <summary>Provides updates on the validity (<c>not null</c>) of <see cref="IP"/>.</summary>
    public virtual IObservable<bool> IPValid => validateableIP.Valid;

    /// <summary>Provides updates informing if this port is endpoint is valid (port and IP <c>not null</c>) or not.</summary>
    public IObservable<bool> Valid => PortValid.CombineLatest(IPValid, (portValid, ipValid) => portValid && ipValid)
                                               .DistinctUntilChanged();

    /// <summary>Gets the latest endpoint value. <see langword="null"/> if <see cref="Port"/> or <see cref="IP"/>
    /// is <see langword="null"/>.</summary>
    public IPEndPoint? LatestEndPoint => IP is not null && Port is not null ? new IPEndPoint(IP, Port.Value) : null;

    /// <summary>Serializes this endpoint in a format suitable for appsettings.json export.</summary>
    /// <returns>A serializable representation of this endpoint's values.</returns>
    public SerializableEndPoint ToSerializable() => new()
    {
        IP = IP?.ToString(),
        Port = Port
    };
}