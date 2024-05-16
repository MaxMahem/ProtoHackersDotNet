using System;
using System.Net;
using ProtoHackersDotNet.Servers.Interface.Client;
using ByteSizeLib;

namespace ProtoHackersDotNet.GUI.MainView;

public class ClientVM(IClient client) : IDisposable
{
    private readonly IClient client = client;
    private readonly DateTime creationTime = DateTime.UtcNow;

    public EndPoint? RemoteEndPoint => client.RemoteEndPoint;
    public IObservable<string?> NameChanges => client.NameChanges;
    public IObservable<ConnectionStatus> ConnectionStatusChanges => client.ConnectionStatusChanges;
    public ConnectionStatus ConnectionStatus => client.ConnectionStatus;
    public IObservable<string> StatusChanges => client.StatusChanges;
    public string? StatusExtended => client.StatusExtended;
    public IObservable<ByteSize> TotalBytesRecievedChanges => client.TotalBytesRecievedChanges;
    public IObservable<ByteSize> TotalBytesTransmittedChanges => client.TotalBytesTransmittedChanges;

    public void Dispose()
    {
        // Dispose any resources if needed
    }
}