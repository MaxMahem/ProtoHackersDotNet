using System.Net;
using System.Net.NetworkInformation;
using Spectre.Console;
using ProtoHackersDotNet.Servers.Interfaces.Server;
using ProtoHackersDotNet.Servers.Interfaces.Client;
using ProtoHackersDotNet.Servers.TcpServer;

static class ServiceSelector
{
    static readonly SelectionPrompt<IServerFactory> ServiceSelectionPrompt = new SelectionPrompt<IServerFactory>()
        .Title("Select the service to run:")
        .AddChoices(ServerRepository.Avaliable)
        .UseConverter(serviceData => serviceData.Name);

    static readonly SelectionPrompt<IPAddress> IpAddressPrompt = new SelectionPrompt<IPAddress>()
        .Title("IP Address:")
        .AddChoices(NetworkInterface.GetAllNetworkInterfaces().Where(netInterface => netInterface.OperationalStatus is OperationalStatus.Up)
                           .SelectMany(netInterface => netInterface.GetIPProperties().UnicastAddresses)
                           .Select(address => address.Address)
                           .Prepend(IPAddress.Any));

    static readonly TextPrompt<ushort> PortPrompt = new TextPrompt<ushort>("Select Port:")
        .ValidationErrorMessage("[red]Select a value between 0 and 65,535.[/]")
        .DefaultValue((ushort) 0);

    public static IServer<IClient> SelectService(out string name)
    {
        var selectedService = AnsiConsole.Prompt(ServiceSelectionPrompt);
        var ipAddress = AnsiConsole.Prompt(IpAddressPrompt);
        var port = AnsiConsole.Prompt(PortPrompt);
        name = selectedService.Name;

        return selectedService.Create(ipAddress, port);
    }
}

