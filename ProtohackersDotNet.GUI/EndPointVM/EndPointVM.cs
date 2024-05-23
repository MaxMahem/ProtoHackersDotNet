using System.Reactive.Subjects;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ProtoHackersDotNet.GUI.MainView;

public abstract partial class EndPointVM : ObservableObject
{
    [ObservableProperty]
    ushort? port;
    partial void OnPortChanged(ushort? value) => this.portValidityObserver.OnNext(value is not null);
    readonly BehaviorSubject<bool> portValidityObserver = new(false);
    public IObservable<bool> PortValid => this.portValidityObserver.AsObservable();

    public abstract IPAddress? IP { get; set; }

    public abstract IObservable<bool> IPValid { get; }

    public EndPointVM(IPAddress? ip, ushort? port) => (Port, IP) = (port, ip);

    public IObservable<bool> Valid => Observable.CombineLatest(PortValid, IPValid, (portValid, ipValid) => portValid && ipValid);

    public IPEndPoint EndPoint => Valid.Latest().First() ? new(IP!, Port!.Value) : ThrowHelper.ThrowArgumentNullException<IPEndPoint>();
}