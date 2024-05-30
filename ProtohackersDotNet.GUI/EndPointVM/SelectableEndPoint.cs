namespace ProtoHackersDotNet.GUI.MainView;

public partial class SelectableEndPoint(IEnumerable<IPAddress> ips, IPAddress? ip, ushort? port) 
    : EndPointVM(ip, port)
{
    public ObservableCollection<IPAddress> SelectableIPs { get; } = new(ips);

    IPAddress _IP = ips.First();
    public override IPAddress? IP {
        get => this._IP;
        set {
            if (value == this._IP) return;
            _ = (value is not null && SelectableIPs.Contains(value)) || ThrowArgumentException<bool>($"{value} is not a selectable IP");
            this._IP = value;
        }
    }

    public override IObservable<bool> IPValid => Observable.Return(true);
}
