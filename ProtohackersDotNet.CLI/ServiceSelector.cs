using System.Net;
using System.Net.NetworkInformation;
using Spectre.Console;
using ProtoHackersDotNet.Servers.Interface.Server;
using ProtoHackersDotNet.Servers.Interface.Client;

static class ServiceSelector
{
    static readonly SelectionPrompt<IServer> ServiceSelectionPrompt = new SelectionPrompt<IServer>()
        .Title("Select the service to run:")
        // .AddChoices(ServerRepository.Avaliable)
        .UseConverter(server => server.Name.Value);

    static readonly SelectionPrompt<IPAddress> IpAddressPrompt = new SelectionPrompt<IPAddress>()
        .Title("IP Address:")
        .AddChoices(NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any));

    static readonly TextPrompt<ushort> PortPrompt = new TextPrompt<ushort>("Select Port:")
        .ValidationErrorMessage("[red]Select a value between 0 and 65,535.[/]")
        .DefaultValue((ushort) 0);
}

