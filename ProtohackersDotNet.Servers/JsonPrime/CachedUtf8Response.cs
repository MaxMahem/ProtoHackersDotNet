using ProtoHackersDotNet.Servers.Helpers;
using ProtoHackersDotNet.Servers.Interface.Client;
using System.Text;

namespace ProtoHackersDotNet.Servers.JsonPrime;

/// <summary>A client message encoded in Utf8, with the string representation cached.</summary>
public struct CachedUtf8Response(string response) : ITransmission
{
    public readonly ReadOnlyMemory<byte> Data { get; } = response.ToBytes(Encoding.UTF8);
    public readonly string Translation => response;
    public readonly bool Broadcast => false;
}