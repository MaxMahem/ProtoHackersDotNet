using ReactiveUI;
using System.Reactive.Disposables;

namespace ProtoHackersDotNet.GUI.MainView.Client;

public sealed class ClientManager : IDisposable
{
    readonly CompositeDisposable disposables;

    readonly ClientVMFactory factory;

    readonly SourceCache<ClientVM, ClientVM> clientStore = new(client => client);

    [SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Binding target")]
    ReadOnlyObservableCollection<ClientVM> clients;

    public ClientManager(ClientVMFactory factory)
    {
        this.factory = factory;

        this.disposables = [ this.clientStore,
            this.clientStore.Connect().ObserveOn(RxApp.MainThreadScheduler)
                .SortAndBind(out this.clients).Subscribe(),
        ];
    }

    public IObservable<int> ActiveClientCount 
        => this.clientStore.Connect().AutoRefreshOnObservable(clientVM => clientVM.Status)
                           .Select(_ => this.clientStore.Items.Where(clientVM => clientVM.IsConnected).Count())
                           .StartWith(0);

    public ReadOnlyObservableCollection<ClientVM> Clients => this.clients;

    public void AddClient(IClient client) 
        => this.clientStore.AddOrUpdate(this.factory.CreateClientVM(client));

    public void ClearDisconnectedClients() 
        => this.clientStore.RemoveKeys(this.clientStore.Items.Where(client => !client.IsConnected));
    public void Dispose() => this.clientStore.Dispose();
}

public sealed class ClientVMFactory(ClientVMFactoryOptions options) {
    public ClientVM CreateClientVM(IClient client) => new(client, options.AgeUpdateInterval);
}