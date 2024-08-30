namespace ProtoHackersDotNet.Servers;

public interface IProblem : IComparable<IProblem>, IEquatable<IProblem>
{
    string Description { get; }
    string Name { get; }
    int Number { get; }
    string ShortName { get; }
    string Title { get; }
}