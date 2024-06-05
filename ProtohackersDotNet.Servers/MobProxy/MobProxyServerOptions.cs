
using System.ComponentModel.DataAnnotations;

namespace ProtoHackersDotNet.Servers.MobProxy;

public sealed class MobProxyServerOptions
{

    [RegularExpression(@"(^| )7[a-zA-Z0-9]{25,34}($| )")]
    public required string ReplacementAddress { get; init; }

    public required UrlEndPoint ChatServer { get; init; } 
}
