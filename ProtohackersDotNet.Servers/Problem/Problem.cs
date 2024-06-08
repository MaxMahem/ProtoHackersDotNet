// use root namespace to avoid name conflict
namespace ProtoHackersDotNet.Servers;

public class Problem : IProblem<Problem>
{
    const string PROBLEM_DIRECTORY = "Problem";

    public int Number { get; }
    public string Name { get; }

    public string Description => descriptionCache.Value;
    readonly Lazy<string> descriptionCache;

    Problem(int number, string shortName, string externalName)
    {
        Number = number;
        Name = externalName;
        this.descriptionCache = new(() => File.ReadAllText(Path.Combine(PROBLEM_DIRECTORY, shortName + ".md")));
    }

    public static readonly Problem Unknown = new(-1, "Testing Problem", "-1: Internal Testing Problem");

    public readonly static Problem Echo         = new(0, "Echo",         "0: Smoke Test");
    public readonly static Problem JsonPrime    = new(1, "JsonPrime",    "1: Prime Time");
    public readonly static Problem PriceTracker = new(2, "PriceTracker", "2: Means to an End");
    public readonly static Problem BudgetChat   = new(3, "BudgetChat",   "3: Budget Chat");
    public readonly static Problem UdpDatabase  = new(4, "UdpDatabase",  "4: Unusual Database Program");
    public readonly static Problem MobProxy     = new(5, "MobProxy",     "5: Mob in the Middle");

    public readonly static IEnumerable<Problem> Problems = [Echo, JsonPrime, PriceTracker, BudgetChat, UdpDatabase, MobProxy];

    public int CompareTo(Problem? other) => Number.CompareTo(other?.Number);
    public bool Equals(Problem? other) => Number.Equals(other?.Number);

    public override bool Equals(object? obj) => obj switch {
        null => false,
        _ when ReferenceEquals(this, obj) => true,
        Problem otherProblem => Equals(otherProblem),
        _ => false,
    };

    public override int GetHashCode() => Number;

    public static bool operator ==(Problem left, Problem right) => left is null ? right is null : left.Equals(right);
    public static bool operator !=(Problem left, Problem right) => !(left == right);
    public static bool operator <(Problem left, Problem right) => left is null ? right is not null : left.CompareTo(right) < 0;
    public static bool operator <=(Problem left, Problem right) => left is null || left.CompareTo(right) <= 0;
    public static bool operator >(Problem left, Problem right) => left is not null && left.CompareTo(right) > 0;
    public static bool operator >=(Problem left, Problem right) => left is null ? right is null : left.CompareTo(right) >= 0;
}
