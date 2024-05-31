using ProtoHackersDotNet.GUI.EndPointVM;
using ProtoHackersDotNet.Helpers;

namespace ProtoHackersDotNet.GUI.MainView;

public abstract class EndPointVM(IPAddress? ip, ushort? port)
{
    readonly ValidateableValue<ushort?> observablePort = ValidateableValue<ushort?>.NotNull(port);
    public ushort? Port {
        get => this.observablePort.Value;
        set => this.observablePort.Value = value;
    }
    public IObservable<bool> PortValid => this.observablePort.Valid;

    readonly ValidateableValue<IPAddress?> validateableIP = ValidateableValue<IPAddress?>.NotNull(ip);
    public virtual IPAddress? IP {
        get => this.validateableIP.Value;
        set => this.validateableIP.Value = value;
    }
    public virtual IObservable<bool> IPValid => validateableIP.Valid;

    public IObservable<bool> Valid => Observable.CombineLatest(PortValid, IPValid, (portValid, ipValid) => portValid && ipValid);

    public IObservable<IPEndPoint> EndPoint => Valid.Where().Select(v => new IPEndPoint(IP!, Port!.Value));
    public IPEndPoint LatestValidEndPoint => EndPoint.Latest().First();

    public SerializableEndPoint ToSerializable() => new(IP?.ToString(), Port);
}