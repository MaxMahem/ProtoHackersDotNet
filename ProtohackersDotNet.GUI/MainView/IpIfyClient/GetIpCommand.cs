using System.Reactive.Subjects;
using ProtoHackersDotNet.GUI.Helpers;

namespace ProtoHackersDotNet.GUI.MainView.IpIfyClient;

public class GetIpCommand(IpIfyClient ipIfyClient) : IObservableCommand<IPAddress>
{
    readonly BehaviorSubject<bool> executing = new(false);
    readonly Subject<IPAddress> externalIP = new();

    public IObservable<bool> CanExecute => executing.AsObservable().Select(b => !b);

    public IObservable<IPAddress> Results => externalIP.AsObservable();

    public async void Execute()
    {
        executing.OnNext(true);
        IPAddress ip = await ipIfyClient.GetExternalIP();
        externalIP.OnNext(ip);
        executing.OnNext(false);
    }
}