using System.Text.Json;

namespace ProtoHackersDotNet.Servers.JsonPrime;

public readonly record struct PrimeResponse(bool Prime)
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Necessary for serialization.")]
    public string Method => "isPrime";

    public readonly static PrimeResponse True  = new(true);
    public readonly static PrimeResponse False = new(false);
}
