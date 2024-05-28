namespace ProtoHackersDotNet.GUI.MainView;

public partial class SelectableEndPoint(IEnumerable<IPAddress> ips, IPAddress? ip, ushort? port) 
    : EndPointVM(ip, port)
{
    ObservableCollection<IPAddress> selectableIPs = new(ips);
    public ObservableCollection<IPAddress> SelectableIPs {
        get => this.selectableIPs;
        set {
            if (value.Count is 0) throw new ArgumentException("Collection must have at least one value");
            this.selectableIPs = value;
            IP = value.First();
        } 
    }

    IPAddress _IP = ips.First();
    public override IPAddress? IP {
        get => _IP;
        set {
            _ = (value is not null && SelectableIPs.Contains(value)) || ThrowHelper.ThrowArgumentException<bool>($"{value} is not a selectable IP");
            _ = SetProperty(ref _IP, value);
        }
    }

    public override IObservable<bool> IPValid => Observable.Return(true);
}
