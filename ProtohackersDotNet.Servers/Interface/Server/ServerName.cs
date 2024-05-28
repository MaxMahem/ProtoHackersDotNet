using Vogen;

namespace ProtoHackersDotNet.Servers.Interface.Server;

[ValueObject<string>, Instance("Unknown", "UnknownServer")]
public readonly partial record struct ServerName;