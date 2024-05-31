using Microsoft.Extensions.Options;
using System.Collections.Specialized;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.MainView.Client;

public sealed class ClientManager
{
    readonly ClientManagerOptions options;

    public ClientManager(IOptions<ClientManagerOptions> options)
    {
        this.options = options.Value;
        ActiveClientCount = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                handler => Clients.CollectionChanged += handler,
                handler => Clients.CollectionChanged -= handler
        ).Select(_ => Clients).SelectMany(clients => clients.Select(client => client.Client.ConnectionStatus)).SelectMany(status => status)
         .Select(_ => Clients.Count(client => client.Client.LatestConnectionStatus is ConnectionStatus.Connected))
         .StartWith(Clients.Count);
    }

    public IObservable<int> ActiveClientCount { get; } 
        
    public ObservableCollection<ClientVM> Clients { get; } = [];

    public void AddClient(IClient client) => Clients.Add(new(client, this.options.AgeUpdateInterval));

    public void ClearDisconnectedClients()
    {
        for (int index = Clients.Count - 1; index >= 0; index--)
            if (Clients[index].Client is { LatestConnectionStatus: not ConnectionStatus.Connected } client) {
                Clients.RemoveAt(index);
                client.Dispose();
            }
    }
}