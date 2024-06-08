namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

public partial class SelectableEndPoint : EndPointVM
{
    public SelectableEndPoint(IEnumerable<IPAddress> ips, IPAddress? ip, ushort? port) : base(ip, port)
    {
        ObservableCollection<IPAddress> collection = new(ips);
        SelectableIPs = new(collection);

        _IP = ips.First();
    }

    public ReadOnlyObservableCollection<IPAddress> SelectableIPs { get; }

    IPAddress _IP;
    public override IPAddress? IP
    {
        get => _IP;
        set
        {
            if (value == _IP) return;
            _ = value is not null && SelectableIPs.Contains(value) || ThrowArgumentException<bool>($"{value} is not a selectable IP");
            _IP = value;
        }
    }

    public override IObservable<bool> IPValid => Observable.Return(true);
}