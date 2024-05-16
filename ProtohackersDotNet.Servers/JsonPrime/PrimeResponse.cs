namespace ProtoHackersDotNet.Servers.JsonPrime;

public readonly record struct PrimeResponse(bool Prime)
{
    [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Necessary for serialization.")]
    public string Method => "isPrime";
}
