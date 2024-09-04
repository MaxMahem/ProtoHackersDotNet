using ProtoHackersDotNet.GUI.Serialization;

namespace ProtoHackersDotNet.GUI.MainView.EndPoint;

public class SelectableEndPoint : EndPointVM
{
    public SelectableEndPoint(IEnumerable<IPAddress> ips, SerializableEndPoint serializableEndPoint) : base(serializableEndPoint)
    {
        ObservableCollection<IPAddress> collection = new(ips);
        SelectableIPs = new(collection);

        IPAddress? ip = IP.Value is null || !collection.Contains(IP.Value) ? ips.FirstOrDefault() : IP.Value;
        IP = ValidateableValue<IPAddress?>.InSet(ips, ip);
    }

    public ReadOnlyObservableCollection<IPAddress> SelectableIPs { get; }
}