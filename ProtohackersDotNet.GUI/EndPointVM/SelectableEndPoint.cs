namespace ProtoHackersDotNet.GUI.MainView;

public partial class SelectableEndPoint(IEnumerable<IPAddress> ips, IPAddress? ip, ushort? port) 
    : EndPointVM(ip, port)
{
    public ObservableCollection<IPAddress> SelectableIPs { get; init; } = new(ips);

    IPAddress _IP = ips.First();
    public override IPAddress? IP {
        get => _IP;
        set {
            _ = (value is not null && SelectableIPs.Contains(value)) || ThrowHelper.ThrowArgumentException<bool>($"{value} is not a selectable IP.");
            _ = SetProperty(ref _IP, value);
        }
    }

    public override IObservable<bool> IPValid => Observable.Return(true);
}
