namespace ProtoHackersDotNet.Servers.JsonPrime;

readonly record struct PrimeQuery
{
    public required string Method { get; init; }
    public required double Number { get; init; }
    public PrimeQuery? Validate() => Method is "isPrime" ? this : null;
}