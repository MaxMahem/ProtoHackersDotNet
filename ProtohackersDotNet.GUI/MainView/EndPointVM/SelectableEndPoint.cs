namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

public class SelectableEndPoint : EndPointVM
{
    public SelectableEndPoint(IEnumerable<IPAddress> ips, IPAddress? ip, ushort? port) : base(ip, port)
    {
        ObservableCollection<IPAddress> collection = new(ips);
        SelectableIPs = new(collection);

        if (ip is null || !collection.Contains(ip)) { ip = ips.FirstOrDefault(); }

        IP = ValidateableValue<IPAddress?>.InSet(ips, ip);
    }

    public ReadOnlyObservableCollection<IPAddress> SelectableIPs { get; }
}