using ProtoHackersDotNet.Servers.Interface.Client;
using ProtoHackersDotNet.Servers.Interface.Server;
using System.Reactive.Linq;
using static CommunityToolkit.Diagnostics.ThrowHelper;

namespace ProtoHackersDotNet.Servers.TcpServer;

public abstract class TcpServer<TServer, TClient>(IPAddress ip, ushort port) : IDisposable
    where TServer : IServer<IClient>
    where TClient : IClient
{
    readonly protected TcpListener listener = new(ip, port);
    CancellationTokenSource? cancellationTokenSource;

    public EndPoint EndPoint => listener.LocalEndpoint;

    readonly Dictionary<Task, TClient> taskClients = [];

    /// <summary>Enumeration of currently active <typeparamref name="TClient"/>.</summary>
    public IEnumerable<TClient> Clients => taskClients.Values;

    protected abstract TServer Instance { get; }

    #region Events

    /// <summary>Notifies subscribers that a <see cref="RemoteConnect"/> event has happened.</summary>
    public event EventHandler<NewClient>? RemoteConnect;

    /// <summary>Notifies subscribers that a <see cref="RemoteDisconnect"/> event has happened.</summary>
    public event EventHandler<RemoteDisconnect>? RemoteDisconnect;

    public event EventHandler<ServerMessage>? ServerEvent;

    /// <summary>Notifies subscribers that a <see cref="ServerEvent"/> has happened.</summary>
    public void NotifyServerEvent(ServerEventType eventType, string message)
        => ServerEvent?.Invoke(this, new() { EndPoint = EndPoint, EventType = eventType, Message = message });

    public event EventHandler<ClientMessage>? ClientEvent;

    #endregion

    public bool Running => listener.Server.IsBound;

    protected abstract TClient CreateClient(TcpClient client, CancellationToken token);

    /// <summary>Starts the server and begins listening for clients.</summary>
    /// <param name="token">A cancellation token that can be used to halt the server.</param>
    /// <returns>A task that represents completion of the listening process.</returns>
    public async Task Start(CancellationToken? token = null)
    {
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token ?? new CancellationToken());

        listener.Start();
        NotifyServerEvent(ServerEventType.Start, $"{TServer.ServerName} started.");

        Task<TcpClient> listenerTask = listener.AcceptTcpClientAsync(cancellationTokenSource.Token).AsTask();

        var tasks = taskClients.Keys.Append(listenerTask);

        try {
            do {
                var completedTask = await Task.WhenAny(tasks);

                switch (completedTask) {
                    // client connect.
                    case Task<TcpClient> completedListenerTask:
                        var client = CreateClient(completedListenerTask.Result, cancellationTokenSource.Token);

                        RemoteConnect?.Invoke(this, new() { Client = client });

                        // start the client handler and add the managing task to the dictionary.
                        var clientTask = client.HandleClient();
                        taskClients[clientTask] = client;

                        // reset the listner.
                        listenerTask = listener.AcceptTcpClientAsync(cancellationTokenSource.Token).AsTask();
                        break;
                    // client disconnect.
                    case Task completedClientTask:
                        var completedClient = taskClients[completedTask];

                        string clientStatus = completedClient.ConnectionStatusChanges.Latest().First().ToString();
                        RemoteDisconnect?.Invoke(this, new() {
                            Client = completedClient,
                            Type = completedClient.ConnectionStatusChanges.Latest().First().ToString(),
                            Message = completedClient.StatusExtended ?? "Client Disconnected.",
                        });

                        completedClient.Dispose();

                        _ = taskClients.Remove(completedTask) || ThrowInvalidOperationException<bool>($"Task not found in dictionary.");
                        break;
                    default:
                        throw new InvalidOperationException();
                }
                tasks = taskClients.Keys.Append(listenerTask);
            } while (!cancellationTokenSource.Token.IsCancellationRequested);
        }
        catch (Exception exception) {
            NotifyServerEvent(ServerEventType.Exception, exception.Message);
        }
        finally {
            DisposeClients();
            listener.Stop();
        }
    }

    /// <summary>Broadcasts <paramref name="message"/> to <paramref name="recepients"/>.</summary>
    public async Task Broadcast<TMessage>(IEnumerable<TClient> recepients, TMessage message)
        where TMessage : ITransmission
    {
        await Task.WhenAll(recepients.Select(client => client.Transmit(message)));
        NotifyServerEvent(ServerEventType.Broadcast, message.Translation);
    }

    void DisposeClients()
    {
        foreach (var client in taskClients.Values)
            client.Dispose();
    }

    public async Task Stop()
    {
        ArgumentNullException.ThrowIfNull(cancellationTokenSource);

        DisposeClients();
        await cancellationTokenSource.CancelAsync();

        listener.Stop();
        NotifyServerEvent(ServerEventType.Stop, $"{TServer.ServerName} stopped.");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        DisposeClients();
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        listener?.Dispose();
    }
}