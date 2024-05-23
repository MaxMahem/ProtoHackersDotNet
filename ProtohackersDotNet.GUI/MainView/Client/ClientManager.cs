using Microsoft.Extensions.Options;
using ProtoHackersDotNet.GUI.MainView.Client.Messages;
using ProtoHackersDotNet.Servers.Interfaces;
using System.Collections.Specialized;
using System.Reactive.Subjects;

namespace ProtoHackersDotNet.GUI.MainView.Client;

public sealed class ClientManager
{
    readonly ClientManagerOptions options;

    Subject<IClient> clientConnectionSubject = new();

    public ClientManager(IOptions<ClientManagerOptions> options)
    {
        this.options = options.Value;
        var clientListChange = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                handler => Clients.CollectionChanged += handler,
                handler => Clients.CollectionChanged -= handler
        ).Select(_ => Clients.Select(client => client.Client));
        var clients = clientListChange.SelectMany(clients => clients);

        ActiveClientCount = clientListChange.Select(clients => clients.Count(client => client.CurrentConnectionStatus is ConnectionStatus.Connected))
                                            .StartWith(Clients.Count);

        // var clients = Clients.Select(client => client.Client);
        var clientTransmissions = clients.Select(clients => clients.Transmissions
            .Select(transmission => new ClientTransmission(clients, transmission))).Merge();
        var clientRecieptions = clients.Select(client => client.Receptions
            .Select(reception => new ClientReception(client, reception))).Merge();
        var clientExceptions = clients.Select(client => client.Exceptions
            .Select(exception => new ClientException(client, exception))).Merge();
        ClientMessages = Observable.Merge<IDisplayMessage>(clientTransmissions, clientRecieptions, clientExceptions);
    }

    public IObservable<int> ActiveClientCount { get; } 
        
    public ObservableCollection<ClientVM> Clients { get; } = [];

    public void AddClient(IClient client) 
    { 
        ClientVM clientVM = new(client, this.options.AgeUpdateInterval);
        Clients.Add(clientVM);
    }

    public void ClearDisconnectedClients()
    {
        for (int index = Clients.Count - 1; index >= 0; index--)
            if (Clients[index].Client.CurrentConnectionStatus is not ConnectionStatus.Connected)
                Clients.RemoveAt(index);
    }

    public IObservable<IDisplayMessage> ClientMessages { get; }
}