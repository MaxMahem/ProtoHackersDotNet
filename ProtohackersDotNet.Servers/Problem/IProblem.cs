namespace ProtoHackersDotNet.Servers;

public interface IProblem<TSelf> : IEquatable<TSelf>, IComparable<TSelf>
{
    string Description { get; }
    string Name { get; }
    int Number { get; }
}