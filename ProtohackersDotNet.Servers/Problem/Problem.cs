// use root namespace to avoid name conflict
namespace ProtoHackersDotNet.Servers;

public abstract record class Problem(int Number, string Name) : IProblem
{
    public abstract string Description { get; }
    public abstract string ShortName { get; }

    public string Title { get; } = $"{Number}: {Name}";

    public static class Types
    {
        public record class Unknown()      : Problem<Unknown>(-1, "Internal Testing Problem");
        public record class Echo()         : Problem<Echo>(0, "Smoke Test");
        public record class JsonPrime()    : Problem<JsonPrime>(1, "Prime Time");
        public record class PriceTracker() : Problem<PriceTracker>(2, "Means to an End");
        public record class BudgetChat()   : Problem<BudgetChat>(3, "Budget Chat");
        public record class UdpDatabase()  : Problem<UdpDatabase>(4, "Unusual Database Program");
        public record class MobProxy()     : Problem<MobProxy>(5, "Mob in the Middle");
    }

    public static class Instances
    {
        public readonly static Types.Unknown Unknown = new();
        public readonly static Types.Echo         Echo         = new();
        public readonly static Types.JsonPrime    JsonPrime    = new();
        public readonly static Types.PriceTracker PriceTracker = new();
        public readonly static Types.BudgetChat   BudgetChat   = new();
        public readonly static Types.UdpDatabase  UdpDatabase  = new();
        public readonly static Types.MobProxy     MobProxy     = new();

        public readonly static IEnumerable<IProblem> All = [
            Echo,
            JsonPrime,
            PriceTracker,
            BudgetChat,
            UdpDatabase,
            MobProxy,
        ];
    }

    public int CompareTo(IProblem? other) => Number.CompareTo(other?.Number);
    public bool Equals(IProblem? other) => Number.Equals(other?.Number);
}

public record class Problem<TSelf>(int Number, string Name) : Problem(Number, Name)
{
    const string PROBLEM_DIRECTORY = "Problem";
    static string MarkdownPath => Path.Combine(PROBLEM_DIRECTORY, typeof(TSelf).Name + ".md");
    static string GetMarkdown() => File.ReadAllText(MarkdownPath);

    public override string Description => descriptionCache.Value;
    readonly Lazy<string> descriptionCache = new(GetMarkdown);

    public override string ShortName { get; } = typeof(TSelf).Name;
}
