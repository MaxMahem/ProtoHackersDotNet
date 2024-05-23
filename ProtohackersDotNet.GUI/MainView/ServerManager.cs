using DynamicData;
using ProtoHackersDotNet.GUI.MainView.Client;
using ProtoHackersDotNet.Servers.Interfaces;
using System.Threading.Tasks;

namespace ProtoHackersDotNet.GUI.MainView;

public class ServerManager
{
    IServer<IClient>? server;

    readonly ObservableCollection<ClientVM> clients = [];

    // public void ClearMessages() => this.messages.Clear();

    //public async Task StartServer(IServerFactory factory, IPAddress ip, ushort port)
    //{
    //    this.server = factory.Create(ip, port);
    //    // this.server.RemoteConnect += OnRemoteConnect;
    //    // this.server.RemoteDisconnect += OnRemoteDisconnect;
    //    // this.server.ServerEvent += (sender, message) => Dispatcher.UIThread.Post(() => PostMessage(message.ToFormated()));
    //
    //    var newClientsObservable = Observable.FromEventPattern<EventHandler<ClientConnection>, ClientConnection>(
    //        handler => this.server.RemoteConnect += handler,
    //        handler => this.server.RemoteConnect -= handler
    //    ).Select(eventPattern => eventPattern.EventArgs.Client);
    //
    //    var newClientMessages = Observable.FromEventPattern<EventHandler<ClientConnection>, IDisplayMessage>(
    //        handler => this.server.RemoteConnect += handler,
    //        handler => this.server.RemoteConnect -= handler
    //    ).Select(eventPattern => eventPattern.EventArgs.ToFormated());
    //
    //    var clientDisconnectMessages = Observable.FromEventPattern<EventHandler<RemoteDisconnect>, IDisplayMessage>(
    //        handler => this.server.RemoteDisconnect += handler,
    //        handler => this.server.RemoteDisconnect -= handler
    //    ).Select(eventPattern => eventPattern.EventArgs.ToFormated());
    //
    //    // newClientsObservable.SelectMany(client => client.DataRecieved)
    //
    //    var messages = newClientMessages.Merge(clientDisconnectMessages);
    //
    //
    //    var serverTask = this.server.Start().ConfigureAwait(false);
    //
    //    //OnPropertyChanged(nameof(ServerRunning));
    //    //OnPropertyChanged(nameof(CanServerStart));
    //
    //    await serverTask;
    //    //// PostMessage(this, new(null, DateTime.Now, EndpointMessageType.SystemStart, SelectedServer.Name));
    //}

    public async Task StopServer()
    {
        if (this.server is null)
            throw new NullReferenceException();

        await this.server.Stop();

        this.server.Dispose();
        this.server = null;

        //this.logStream?.Flush();
        //this.logStream?.Dispose();
        //this.logFileStream?.Dispose();

        //OnPropertyChanged(nameof(ServerRunning));
        //OnPropertyChanged(nameof(CanServerStart));
    }
}